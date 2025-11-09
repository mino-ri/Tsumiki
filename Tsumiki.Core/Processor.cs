using Tsumiki.Metadata;

namespace Tsumiki.Core;

internal struct Voice
{
    public SynthVoice SynthVoice;
    public CarrierWave Carrier;
    public ModulatorWave Modulator;
    public Envelope Envelope1;
    public Envelope Envelope2;
    public ResonantLowPassFilterConfig FilterConfig;
    public ResonantLowPassFilter Filter;
}

public class Processor()
{
    const int MaxVoices = 6;
    private MidiVoiceContainer _container = new(0);
    private Voice[] _voices = [];
    private CarrierWaveConfig _carrierWaveConfig;
    private ModulatorWaveConfig _modulatorWaveConfig;
    private EnvelopeConfig _envelope1Config;
    private EnvelopeConfig _envelope2Config;

    public bool IsActive => _voices.Length > 0;

    [InitTiming]
    public void OnActive(bool isActive)
    {
        if (isActive)
        {
            _container = new MidiVoiceContainer(MaxVoices);
            _voices = GC.AllocateArray<Voice>(MaxVoices, true);
        }
        else
        {
            _container = new(0);
            _voices = [];
        }
    }

    [InitTiming]
    public void Recalculate(ITsumikiModel model, double sampleRate)
    {
        _carrierWaveConfig = new CarrierWaveConfig(model.A1);
        _modulatorWaveConfig = new ModulatorWaveConfig(model.A2);
        _envelope1Config = new EnvelopeConfig(model.A1, sampleRate);
        _envelope2Config = new EnvelopeConfig(model.A2, sampleRate);
    }

    [EventTiming]
    public void ReserveNote(in MidiEventReservation<MidiNote> reservation)
    {
        _container.Reserve(in reservation);
    }

    [AudioTiming]
    public void ProcessMain(ITsumikiModel model, double sampleRate, int sampleCount, Span<float> leftOutput, Span<float> rightOutput)
    {
        var masterVolume = model.Master;
        var filterMix = model.Filter.Mix;
        var passVolume = 1f - filterMix;
        var pitchBend = model.PitchBend * model.Input.Bend;

        for (var sample = 0; sample < sampleCount; sample++)
        {
            var output = 0f;
            _container.Tick();
            for (var i = 0; i < MaxVoices; i++)
            {
                ref var voice = ref _voices[i];
                // イベントタイミングの処理を発火するフラグ
                switch (voice.SynthVoice.Tick(in _container.Voices[i], pitchBend, sampleRate))
                {
                    case VoiceEvent.StartNote:
                        // EVENT CALL
                        StartNote(ref voice, model, sampleRate);
                        break;
                    case VoiceEvent.RestartNote:
                        // EVENT CALL
                        RestartNote(ref voice, model, sampleRate);
                        break;
                }

                if (voice.SynthVoice.State != VoiceState.Inactive)
                {
                    var noteOn = voice.SynthVoice.State == VoiceState.Active;
                    var level1 = voice.Envelope1.TickAndRender(in _envelope1Config, noteOn);
                    if (voice.SynthVoice.State == VoiceState.Release && level1 == 0)
                    {
                        voice.SynthVoice.State = VoiceState.Inactive;
                        continue;
                    }
                    var lavel2 = voice.Envelope2.TickAndRender(in _envelope2Config, noteOn);
                    var fm = lavel2 * voice.Modulator.TickAndRender(in _modulatorWaveConfig, voice.SynthVoice.Delta, -1.0);
                    var voiceOutput = level1 * voice.Carrier.TickAndRender(in _carrierWaveConfig, voice.SynthVoice.Delta, -1.0, fm);

                    output += filterMix == 0f
                        ? voiceOutput
                        : passVolume * voiceOutput + filterMix * voice.Filter.TickAndRender(in voice.FilterConfig, voiceOutput);
                }
            }

            output *= masterVolume;
            leftOutput[sample] = output;
            rightOutput[sample] = output;
        }
    }

    [EventTiming]
    private void StartNote(ref Voice voice, ITsumikiModel model, double sampleRate)
    {
        voice.Carrier.Reset(in _carrierWaveConfig);
        voice.Modulator.Reset(in _modulatorWaveConfig);
        voice.Envelope2.Reset();
        voice.FilterConfig = new ResonantLowPassFilterConfig(model.Filter, voice.SynthVoice.Pitch, sampleRate);
        voice.Filter.Reset();
    }

    [EventTiming]
    private void RestartNote(ref Voice voice, ITsumikiModel model, double sampleRate)
    {
        voice.Envelope1.Restart();
        voice.Envelope2.Restart();
        voice.FilterConfig = new ResonantLowPassFilterConfig(model.Filter, voice.SynthVoice.Pitch, sampleRate);
    }
}
