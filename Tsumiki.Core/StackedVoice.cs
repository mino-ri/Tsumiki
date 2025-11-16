using Tsumiki.Metadata;

namespace Tsumiki.Core;

/// <summary>
/// ユニゾン/倍音を統合した「スタック機能」の最大数だけ並んだ固定長配列。プログラミング用語の Stack とは無関係なので注意。
/// </summary>
[System.Runtime.CompilerServices.InlineArray(MathT.MaxStackCount)]
internal struct Stacked<T>
{
    private T _item;
}

[EventTiming]
internal struct ConfigSet
{
    public CarrierWaveConfig CarrierWave;
    public ModulatorWaveConfig ModulatorWave;
    public EnvelopeConfig Envelope1;
    public EnvelopeConfig Envelope2;
    public StackConfig Stack;
    public VoiceConfig VoceConfig;

    [EventTiming]
    public void Recalculate(ITsumikiModel model, double sampleRate)
    {
        CarrierWave = new CarrierWaveConfig(model.A1);
        ModulatorWave = new ModulatorWaveConfig(model.A2);
        Envelope1 = new EnvelopeConfig(model.A1, sampleRate);
        Envelope2 = new EnvelopeConfig(model.A2, sampleRate);
        Stack = new StackConfig(model.Input);
        VoceConfig = new VoiceConfig(model.Input, sampleRate);
    }
}

internal struct VoiceConfig(IInputUnit unit, double sampleRate)
{
    public bool Enable = unit.Glide > 0;
    public bool Polyphony = unit.Glide < 0;
    public FilterConfigD Filter = unit.Glide > 0
        ? new FilterConfigD(50 - unit.Glide, sampleRate)
        : default;
}

[EventTiming]
internal readonly struct StackConfig
{
    private static readonly int[][] DetuneFactors =
    [
        [],
        [ 0 ],
        [ -1, 1 ],
        [ 0, -4, 4 ],
        [ -1, 1, -9, 9 ],
        [ 0, -4, 4, -16, 16 ],
        [ -1, 1, -9, 9, -25, 25 ],
        [ 0, -4, 4, -16, 16, 36, -36 ],
    ];

    private static readonly int[][] PanFactors =
    [
        [],
        [ 0 ],
        [ -1, 1 ],
        [ 0, -2, 2 ],
        [ -1, 1, 3, -3 ],
        [ 0, -2, 2, 4, -4 ],
        [ -1, 1, 3, -3, -5, 5 ],
        [ 0, -2, 2, 4, -4, -6, 6 ],
    ];

    public readonly int Stack;
    public readonly Stacked<double> Pitches;
    public readonly Stacked<float> Pans;

    [EventTiming]
    public StackConfig(IInputUnit unit)
    {
        Stack = unit.Stack;
        if (unit.StackMode == StackMode.Unison)
        {
            var detune = unit.StackDetune;
            var factor = DetuneFactors[Stack];
            for (var i = 0; i < Stack; i++)
            {
                Pitches[i] = Math.Pow(2.0, detune * factor[i] / 48000.0);
            }
        }
        else
        {
            for (var i = 0; i < Stack; i++)
            {
                Pitches[i] = i + 1;
            }
            Pitches[MathT.MaxStackCount - 1] = 8;
        }

        var stereo = unit.StackStereo;
        if (stereo == 0f)
        {
            ((Span<float>)Pans).Clear();
        }
        else
        {
            var factor = PanFactors[Stack];
            for (var i = 0; i < Stack; i++)
            {
                Pans[i] = factor[i] * stereo / Stack;
            }
        }
    }
}

/// <summary>毎フレーム渡される設定値。</summary>
[EventTiming]
[method: EventTiming]
internal struct TickConfig(double pitchBend, double sampleRate, float filterMix, float pan)
{
    public double PitchBend = pitchBend;
    public double SampleRate = sampleRate;
    public bool UseFilter = filterMix != 0f;
    public float FilterMix = filterMix;
    public float FilterSource = 1f - filterMix;
    public float Pan = pan;
}

/// <summary>スタック機能を適用した後の音声出力器。</summary>
[EventTiming]
internal struct StackedVoice
{
    public SynthVoice SynthVoice;
    public Stacked<ResetPulse> Pulses;
    public Stacked<CarrierWave> Carriers;
    public Stacked<ModulatorWave> Modulators;
    public Envelope Envelope1;
    public Envelope Envelope2;
    public ResonantLowPassFilterConfig FilterConfig;
    public ResonantLowPassFilter LeftFilter;
    public ResonantLowPassFilter RightFilter;

    [AudioTiming]
    public (float left, float right) TickAndRender(in ConfigSet config, in MidiVoice midi, in TickConfig tick, ITsumikiModel model)
    {
        switch (SynthVoice.Tick(in midi, in config.VoceConfig, tick.PitchBend, tick.SampleRate))
        {
            case VoiceEvent.StartNote:
                // EVENT CALL
                StartNote(in config, model, tick.SampleRate);
                break;
            case VoiceEvent.RestartNote:
                // EVENT CALL
                RestartNote(model, tick.SampleRate);
                break;
        }

        if (SynthVoice.State == VoiceState.Inactive)
            return default;

        var noteOn = SynthVoice.State == VoiceState.Active;
        var level1 = Envelope1.TickAndRender(in config.Envelope1, noteOn);
        if (SynthVoice.State == VoiceState.Release && level1 == 0)
        {
            SynthVoice.State = VoiceState.Inactive;
            return default;
        }

        var level2 = Envelope2.TickAndRender(in config.Envelope2, noteOn);
        var left = 0f;
        var right = 0f;
        for (var i = 0; i < config.Stack.Stack; i++)
        {
            var delta = SynthVoice.Delta * config.Stack.Pitches[i];
            var resetPhase = Pulses[i].Tick(delta);
            var fm = level2 * Modulators[i].TickAndRender(in config.ModulatorWave, delta, resetPhase);
            var voiceOutput = level1 * Carriers[i].TickAndRender(in config.CarrierWave, delta, resetPhase, fm);
            var (lPan, rPan) = MathT.GetPanLevel(tick.Pan + config.Stack.Pans[i]);
            left += voiceOutput * lPan;
            right += voiceOutput * rPan;
        }

        left /= config.Stack.Stack;
        right /= config.Stack.Stack;

        if (tick.UseFilter)
        {
            return (tick.FilterSource * left + tick.FilterMix * LeftFilter.TickAndRender(in FilterConfig, left),
                tick.FilterSource * right + tick.FilterMix * RightFilter.TickAndRender(in FilterConfig, right));
        }
        else
        {
            return (left, right);
        }
    }

    [EventTiming]
    private void StartNote(in ConfigSet config, ITsumikiModel model, double sampleRate)
    {
        for (var i = 0; i < config.Stack.Stack; i++)
        {
            Carriers[i].Reset(in config.CarrierWave);
            Modulators[i].Reset(in config.ModulatorWave);
            Pulses[i].Reset();
        }

        Envelope2.Reset();
        FilterConfig = new ResonantLowPassFilterConfig(model.Filter, SynthVoice.Pitch, sampleRate);
        LeftFilter.Reset();
        RightFilter.Reset();
    }

    [EventTiming]
    private void RestartNote(ITsumikiModel model, double sampleRate)
    {
        Envelope1.Restart();
        Envelope2.Restart();
        FilterConfig = new ResonantLowPassFilterConfig(model.Filter, SynthVoice.Pitch, sampleRate);
    }
}
