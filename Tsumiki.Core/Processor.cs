using Tsumiki.Metadata;

namespace Tsumiki.Core;

internal struct Voice
{
    public SynthVoice SynthVoice;
    public ModulatorWave Oscillator;
    public Envelope Envelope;
}

public class Processor()
{
    const int MaxVoices = 6;
    private MidiVoiceContainer _container = new(0);
    private Voice[] _voices = [];
    private ModulatorWaveConfig _modulatorWaveConfig;
    private EnvelopeConfig _envelopeConfig;

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
        _modulatorWaveConfig = new ModulatorWaveConfig(model.A2);
        _envelopeConfig = new EnvelopeConfig(model.A2, sampleRate);
    }

    [EventTiming]
    public void ReserveNote(in MidiEventReservation<MidiNote> reservation)
    {
        _container.Reserve(in reservation);
    }

    [AudioTiming]
    public void ProcessMain(ITsumikiModel model, double sampleRate, int sampleCount, Span<float> leftOutput, Span<float> rightOutput)
    {
        _modulatorWaveConfig = new ModulatorWaveConfig(model.A2);
        _envelopeConfig = new EnvelopeConfig(model.A2, sampleRate);

        var masterVolume = model.Master;
        var pitchBend = model.PitchBend * model.Input.Bend;

        for (var sample = 0; sample < sampleCount; sample++)
        {
            var output = 0f;
            _container.Tick();
            for (var i = 0; i < MaxVoices; i++)
            {
                ref var voice = ref _voices[i];
                var executeEvent = voice.SynthVoice.Tick(in _container.Voices[i], pitchBend, sampleRate);
                // イベントタイミングの処理を発火するフラグ
                if (executeEvent)
                {
                    // EVENT CALL
                    EventCall(i);
                }

                if (voice.SynthVoice.State != VoiceState.Inactive)
                {
                    var level = voice.Envelope.TickAndRender(in _envelopeConfig, voice.SynthVoice.State == VoiceState.Active);
                    if (voice.SynthVoice.State == VoiceState.Release && level == 0)
                    {
                        voice.SynthVoice.State = VoiceState.Inactive;
                        continue;
                    }

                    output += level * voice.Oscillator.TickAndRender(in _modulatorWaveConfig, voice.SynthVoice.Delta, -1.0);
                }
            }

            output *= masterVolume;
            leftOutput[sample] = output;
            rightOutput[sample] = output;
        }
    }

    [EventTiming]
    private void EventCall(int voiceIndex)
    {
        _voices[voiceIndex].Oscillator.Reset(in _modulatorWaveConfig);
    }
}
