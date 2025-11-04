using NPlug;

namespace Tsumiki;

public class SampleMidiModel : AudioProcessorModel
{
    public AudioParameter Gain { get; }

    public SampleMidiModel() : base("Tsumiki")
    {
        AddByPassParameter();
        Gain = AddParameter(new AudioParameter("Gain", units: "dB", defaultNormalizedValue: 0.7));
    }
}

public class SampleMidiController : AudioController<SampleMidiModel>
{
    public static readonly Guid ClassId = new("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");
}

public class SampleMidiProcessor()
    : AudioProcessor<SampleMidiModel>(AudioSampleSizeSupport.Float32)
{
    private SynthVoice[] _voices = [];
    private const int MaxVoices = 16;

    public static readonly Guid ClassId = new("f6e5d4c3-b2a1-4d5e-9f8e-7d6c5b4a3210");

    public override Guid ControllerClassId => SampleMidiController.ClassId;

    protected override bool Initialize(AudioHostApplication host)
    {
        // Synthesizer has no audio input, only MIDI input and audio output
        AddDefaultStereoAudioOutput();
        AddEventInput("MIDI Input", 16);
        return true;
    }

    protected override void OnActivate(bool isActive)
    {
        if (isActive)
        {
            _voices = new SynthVoice[MaxVoices];
            for (int i = 0; i < MaxVoices; i++)
            {
                _voices[i] = new SynthVoice();
            }
        }
        else
        {
            _voices = [];
        }
    }

    protected override void ProcessEvent(in AudioEvent audioEvent)
    {
        switch (audioEvent.Kind)
        {
            case AudioEventKind.NoteOn:
                var noteOn = audioEvent.Value.NoteOn;
                HandleNoteOn(noteOn.Pitch, noteOn.Velocity, noteOn.NoteId, audioEvent.SampleOffset);
                break;

            case AudioEventKind.NoteOff:
                var noteOff = audioEvent.Value.NoteOff;
                HandleNoteOff(noteOff.Pitch, noteOff.NoteId, audioEvent.SampleOffset);
                break;
        }
    }

    private void HandleNoteOn(short pitch, float velocity, int noteId, int sampleOffset)
    {
        // Find an inactive voice
        SynthVoice? voice = null;
        foreach (var v in _voices)
        {
            if (v.IsInactive())
            {
                voice = v;
                break;
            }
        }

        // If no inactive voice found, steal the first voice (simple voice stealing)
        voice ??= _voices[0];

        voice.NoteOn(pitch, velocity, noteId, ProcessSetupData.SampleRate, sampleOffset);
    }

    private void HandleNoteOff(short pitch, int noteId, int sampleOffset)
    {
        // Release all voices matching this note
        foreach (var voice in _voices)
        {
            if (!voice.IsInactive() &&
                (voice.NoteId == noteId || noteId == -1 && voice.NoteNumber == pitch))
            {
                voice.NoteOff(sampleOffset);
            }
        }
    }

    protected override void ProcessMain(in AudioProcessData data)
    {
        if (_voices.Length == 0)
            return;

        // Get stereo output buffers
        var leftChannel = data.Output[0].GetChannelSpanAsFloat32(ProcessSetupData, data, 0);
        var rightChannel = data.Output[0].GetChannelSpanAsFloat32(ProcessSetupData, data, 1);

        var sampleCount = data.SampleCount;
        var gain = (float)Model.Gain.NormalizedValue;

        // Generate audio for each sample
        for (int sample = 0; sample < sampleCount; sample++)
        {
            float output = 0.0f;

            // Sum all voice outputs
            foreach (var voice in _voices)
            {
                output += voice.RenderSample();
            }

            // Normalize by voice count to prevent clipping and apply gain
            output = output / MaxVoices * gain;

            // Mono to stereo
            leftChannel[sample] = output;
            rightChannel[sample] = output;
        }
    }

    private class SynthVoice
    {
        private enum VoiceState { Inactive, Pending, Active, Release }

        private VoiceState _state;
        private double _phase;
        private double _phaseIncrement;
        private float _currentVolume;
        private float _targetVolume;
        private int _delaySamples;
        private const float VolumeSmoothing = 0.001f;

        public int NoteNumber { get; private set; }
        public int NoteId { get; private set; }

        public SynthVoice()
        {
            _state = VoiceState.Inactive;
            _phase = 0.0;
            _phaseIncrement = 0.0;
            _currentVolume = 0.0f;
            _targetVolume = 0.0f;
            _delaySamples = 0;
            NoteNumber = -1;
            NoteId = -1;
        }

        public bool IsInactive() => _state == VoiceState.Inactive;

        public void NoteOn(short noteNumber, float velocity, int noteId, double sampleRate, int sampleOffset)
        {
            NoteNumber = noteNumber;
            NoteId = noteId;

            // Calculate frequency: f = 440 × 2^((noteNumber - 69) / 12)
            // MIDI note 69 = A4 = 440Hz
            double frequency = 440.0 * Math.Pow(2.0, (noteNumber - 69) / 12.0);

            // Calculate phase increment for each sample
            _phaseIncrement = 2.0 * Math.PI * frequency / sampleRate;

            // Reset phase for clean start
            _phase = 0.0;

            // Set volume based on velocity
            _targetVolume = velocity;

            // Set delay based on sample offset
            _delaySamples = sampleOffset;
            _state = _delaySamples > 0 ? VoiceState.Pending : VoiceState.Active;
        }

        public void NoteOff(int sampleOffset)
        {
            // Set delay for note off
            _delaySamples = sampleOffset;
            _targetVolume = 0.0f;

            if (_state == VoiceState.Active && _delaySamples == 0)
            {
                _state = VoiceState.Release;
            }
        }

        public float RenderSample()
        {
            // Handle delay countdown
            if (_delaySamples > 0)
            {
                _delaySamples--;
                if (_delaySamples == 0)
                {
                    // Activate or release based on current state
                    if (_state == VoiceState.Pending)
                    {
                        _state = VoiceState.Active;
                    }
                    else if (_targetVolume == 0.0f)
                    {
                        _state = VoiceState.Release;
                    }
                }

                // Don't output audio while waiting
                if (_state == VoiceState.Pending)
                    return 0.0f;
            }

            if (_state == VoiceState.Inactive)
                return 0.0f;

            // Smooth volume changes to prevent clicks
            _currentVolume += (_targetVolume - _currentVolume) * VolumeSmoothing;

            // Generate sine wave
            float output = (float)Math.Sin(_phase) * _currentVolume;

            // Advance phase
            _phase += _phaseIncrement;

            // Wrap phase to prevent precision issues
            if (_phase >= 2.0 * Math.PI)
            {
                _phase -= 2.0 * Math.PI;
            }

            // Deactivate voice when volume reaches zero during release
            if (_state == VoiceState.Release && Math.Abs(_currentVolume) < 0.0001f)
            {
                _state = VoiceState.Inactive;
                NoteNumber = -1;
                NoteId = -1;
            }

            return output;
        }
    }
}
