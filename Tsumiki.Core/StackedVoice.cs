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
    public readonly int Stack;
    public readonly Stacked<double> Pitches;

    [EventTiming]
    public StackConfig(IInputUnit unit)
    {
        Stack = unit.Stack;
        if (unit.StackMode == StackMode.Unison)
        {
            var halfCent = -unit.StackDetune * (Stack - 1);
            var step = unit.StackDetune * 2;
            for (var i = 0; i < Stack; i++)
            {
                Pitches[i] = Math.Pow(2.0, halfCent / 24000.0);
                halfCent += step;
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
    }
}

[EventTiming]
[method: EventTiming]
internal readonly struct TickConfig(double pitchBend, double sampleRate, float filterMix)
{
    public readonly double PitchBend = pitchBend;
    public readonly double SampleRate = sampleRate;
    public readonly bool UseFilter = filterMix != 0f;
    public readonly float FilterMix = filterMix;
    public readonly float FilterSource = 1f - filterMix;
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
    public ResonantLowPassFilter Filter;

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
        var voiceOutput = 0f;
        for (var i = 0; i < config.Stack.Stack; i++)
        {
            var delta = SynthVoice.Delta * config.Stack.Pitches[i];
            var fm = lavel2 * Modulator[i].TickAndRender(in config.ModulatorWave, delta, -1.0);
            voiceOutput += level1 * Carrier[i].TickAndRender(in config.CarrierWave, delta, -1.0, fm);
        }
        voiceOutput /= config.Stack.Stack;

        var output = tick.UseFilter
            ? tick.FilterSource * voiceOutput + tick.FilterMix * Filter.TickAndRender(in FilterConfig, voiceOutput)
            : voiceOutput;
        return (output, output);
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
        Filter.Reset();
    }

    [EventTiming]
    private void RestartNote(ITsumikiModel model, double sampleRate)
    {
        Envelope1.Restart();
        Envelope2.Restart();
        FilterConfig = new ResonantLowPassFilterConfig(model.Filter, SynthVoice.Pitch, sampleRate);
    }
}
