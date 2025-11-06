namespace Tsumiki.Core;

internal enum VoiceState
{
    Inactive = 0,
    Pending = 1,
    Active = 2,
    Release = 3,
}

internal struct SynthVoice
{
    VoiceState _state;
    float _phase;
    float _phaseIncrement;
    float _currentVolume;
    float _targetVolume;
    int _delaySamples;

    public int NoteNumber { get; private set; }
    public int NoteId { get; private set; }

    public readonly bool IsInactive => _state == VoiceState.Inactive;

    public void NoteOn(int noteNumber, float velocity, int noteId, double sampleRate, int sampleOffset)
    {
        NoteNumber = noteNumber;
        NoteId = noteId;
        var frequency = 440.0 * Math.Pow(2.0, (noteNumber - 69.0) / 12.0);
        _phaseIncrement = (float)(frequency / sampleRate);
        _phase = 0f;
        _targetVolume = velocity;
        _delaySamples = sampleOffset;
        _state = sampleOffset > 0 ? VoiceState.Pending : VoiceState.Active;
    }

    public void NoteOff(int sampleOffset)
    {
        _delaySamples = sampleOffset;
        _targetVolume = 0f;
        if (_state == VoiceState.Active && sampleOffset == 0)
            _state = VoiceState.Release;
    }

    public float RenderSample()
    {
        if (_delaySamples > 0)
        {
            _delaySamples--;
            if (_delaySamples == 0)
            {
                _state = _state == VoiceState.Pending ? VoiceState.Active
                    : _targetVolume == 0f ? VoiceState.Release
                    : _state;
            }
        }
        if (_state is VoiceState.Pending or VoiceState.Inactive)
        {
            return 0f;
        }

        _currentVolume += (_targetVolume - _currentVolume) / 1024f;
        var output = MathT.Sin(_phase) * _currentVolume;
        _phase += _phaseIncrement;
        if (_phase >= 1f)
        {
            _phase -= 1f;
        }

        if (_state == VoiceState.Release && Math.Abs(_currentVolume) < 0.0001f)
        {
            _state = VoiceState.Inactive;
            _currentVolume = 0;
            NoteNumber = -1;
            NoteId = -1;
        }

        return output;
    }

    public void Reset()
    {
        NoteNumber = -1;
        NoteId = -1;
    }
}

public class TsumikiProcessor()
{
    const int MaxVoices = 16;
    SynthVoice[] _voices = [];

    public bool IsActive => _voices.Length > 0;

    public void OnActive(bool isActive)
    {
        if (isActive)
        {
            _voices = GC.AllocateArray<SynthVoice>(MaxVoices, true);
            for (var i = 0; i < MaxVoices; i++)
            {
                _voices[i].Reset();
            }
        }
        else
        {
            _voices = [];
        }
    }

    public int GetInactiveIndex()
    {
        for (var i = 0; i < MaxVoices; i++)
        {
            if (_voices[i].IsInactive)
            {
                return i;
            }
        }

        return 0;
    }

    public void OnNoteOn(int pitch, float velocity, int noteId, double sampleRate, int sampleOffset)
    {
        var index = GetInactiveIndex();
        _voices[index].NoteOn(pitch, velocity, noteId, sampleRate, sampleOffset);
    }

    public void OnNoteOff(int pitch, int noteId, int sampleOffset)
    {
        for (var i = 0; i < MaxVoices; i++)
        {
            if (!_voices[i].IsInactive && _voices[i].NoteId == noteId || noteId == -1 && _voices[i].NoteNumber == pitch)
                _voices[i].NoteOff(sampleOffset);
        }
    }

    public void ProcessMain(ITsumikiModel model, int sampleCount, Span<float> leftOutput, Span<float> rightOutput)
    {
        var gain = model.Gain;
        for (var sample = 0; sample < sampleCount; sample++)
        {
            var output = 0f;
            for (var i = 0; i < MaxVoices; i++)
            {
                output += _voices[i].RenderSample();
            }

            output = output / MaxVoices * gain;
            leftOutput[sample] = output;
            rightOutput[sample] = output;
        }
    }
}
