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
    public OscillatorConfig OscillatorA;
    public OscillatorConfig OscillatorB;
    public StackConfig Stack;
    public VoiceConfig VoceConfig;

    [EventTiming]
    public void Recalculate(ITsumikiModel model, double sampleRate)
    {
        OscillatorA.RecalculateA(model, sampleRate);
        OscillatorB.RecalculateB(model, sampleRate);
        Stack = new StackConfig(model.Input);
        VoceConfig = new VoiceConfig(model.Input, sampleRate);
    }
}

[EventTiming]
internal struct OscillatorConfig
{
    public CarrierWaveConfig CarrierWave;
    public ModulatorWaveConfig ModulatorWave;
    public EnvelopeConfig Envelope1;
    public EnvelopeConfig Envelope2;
    public float Pan;

    [EventTiming]
    public void RecalculateA(ITsumikiModel model, double sampleRate)
    {
        CarrierWave = new CarrierWaveConfig(model.A1);
        ModulatorWave = new ModulatorWaveConfig(model.A2);
        Envelope1 = new EnvelopeConfig(model.A1, sampleRate);
        Envelope2 = new EnvelopeConfig(model.A2, sampleRate);
        Pan = model.A1.Pan;
    }

    [EventTiming]
    public void RecalculateB(ITsumikiModel model, double sampleRate)
    {
        CarrierWave = new CarrierWaveConfig(model.B1);
        ModulatorWave = new ModulatorWaveConfig(model.B2);
        Envelope1 = new EnvelopeConfig(model.B1, sampleRate);
        Envelope2 = new EnvelopeConfig(model.B2, sampleRate);
        Pan = model.B1.Pan;
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
internal struct TickConfig(double pitchBend, double sampleRate, float filterMix)
{
    public double PitchBend = pitchBend;
    public double SampleRate = sampleRate;
    public bool UseFilter = filterMix != 0f;
    public float FilterMix = filterMix;
    public float FilterSource = 1f - filterMix;
}

/// <summary>スタック機能を適用した後の音声出力器。</summary>
[EventTiming]
internal struct StackedVoice
{
    public SynthVoice SynthVoice;
    public StackedOscillator OscillatorA;
    public StackedOscillator OscillatorB;
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
            case VoiceEvent.PitchChanged:
                FilterConfig.RecalculatePitch(SynthVoice.Pitch, tick.SampleRate);
                break;
        }

        if (SynthVoice.State == VoiceState.Inactive)
            return default;

        var noteOn = SynthVoice.State == VoiceState.Active;
        var (leftA, rightA, levelA) = OscillatorA.TickAndRender(in config.OscillatorA, in config.Stack, noteOn, SynthVoice.Delta);
        var (leftB, rightB, levelB) = OscillatorB.TickAndRender(in config.OscillatorB, in config.Stack, noteOn, SynthVoice.Delta);
        if (SynthVoice.State == VoiceState.Release && levelA == 0f && levelB == 0f)
        {
            SynthVoice.State = VoiceState.Inactive;
            return default;
        }

        var left = (leftA + leftB) / config.Stack.Stack;
        var right = (rightA + rightB) / config.Stack.Stack;

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
        OscillatorA.StartNote(in config.OscillatorA, in config.Stack);
        OscillatorB.StartNote(in config.OscillatorB, in config.Stack);
        FilterConfig = new ResonantLowPassFilterConfig(model.Filter, SynthVoice.Pitch, sampleRate);
        LeftFilter.Reset();
        RightFilter.Reset();
    }

    [EventTiming]
    private void RestartNote(ITsumikiModel model, double sampleRate)
    {
        OscillatorA.RestartNote();
        OscillatorB.RestartNote();
        FilterConfig = new ResonantLowPassFilterConfig(model.Filter, SynthVoice.Pitch, sampleRate);
    }
}
