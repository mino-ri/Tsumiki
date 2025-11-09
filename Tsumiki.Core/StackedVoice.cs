using Tsumiki.Metadata;

namespace Tsumiki.Core;

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

    [EventTiming]
    public void Recalculate(ITsumikiModel model, double sampleRate)
    {
        CarrierWave = new CarrierWaveConfig(model.A1);
        ModulatorWave = new ModulatorWaveConfig(model.A2);
        Envelope1 = new EnvelopeConfig(model.A1, sampleRate);
        Envelope2 = new EnvelopeConfig(model.A2, sampleRate);
        Stack = new StackConfig(model.Input);
    }
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

[EventTiming]
internal struct StackedVoice
{
    public SynthVoice SynthVoice;
    public Stacked<CarrierWave> Carrier;
    public Stacked<ModulatorWave> Modulator;
    public Envelope Envelope1;
    public Envelope Envelope2;
    public ResonantLowPassFilterConfig FilterConfig;
    public ResonantLowPassFilter LeftFilter;
    public ResonantLowPassFilter RightFilter;

    [AudioTiming]
    public (float left, float right) TickAndRender(in ConfigSet config, in MidiVoice midi, in TickConfig tick, ITsumikiModel model)
    {
        switch (SynthVoice.Tick(in midi, tick.PitchBend, tick.SampleRate))
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

        var lavel2 = Envelope2.TickAndRender(in config.Envelope2, noteOn);
        var left = 0f;
        var right = 0f;
        for (var i = 0; i < config.Stack.Stack; i++)
        {
            var delta = SynthVoice.Delta * config.Stack.Pitches[i];
            var fm = lavel2 * Modulator[i].TickAndRender(in config.ModulatorWave, delta, -1.0);
            var voiceOutput = level1 * Carrier[i].TickAndRender(in config.CarrierWave, delta, -1.0, fm);
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
            Carrier[i].Reset(in config.CarrierWave);
            Modulator[i].Reset(in config.ModulatorWave);
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
