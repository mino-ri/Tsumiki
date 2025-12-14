using Tsumiki.Metadata;

namespace Tsumiki.Core;

internal enum VoiceState
{
    Inactive = 0,
    Active = 1,
    Release = 2,
}

internal enum VoiceEvent
{
    /// <summary>イベントは発生しない</summary>
    None = 0,
    /// <summary>無音状態から発音状態に移行する</summary>
    StartNote = 1,
    /// <summary>リリース途中から発音状態に移行する</summary>
    RestartNote = 2,
    /// <summary>発音中にピッチが変化した</summary>
    PitchChanged = 3,
}

[AudioTiming]
internal struct SynthVoice(GliderConfig glideConfig)
{
    private readonly GliderConfig _glideConfig = glideConfig;
    public VoiceState State;
    public float Velocity;
    public float PolyPressure;
    public double Pitch;
    public double Delta;
    private double _targetPitch;
    private double _targetVelocity;
    private LowPassFilterD _pitchGlider;
    private LowPassFilterD _velocityGlider;

    [AudioTiming]
    public VoiceEvent Tick(in MidiVoice midi, double pitchBend)
    {
        VoiceEvent result;
        if (midi.Note.IsOn)
        {
            var oldState = State;
            State = VoiceState.Active;
            _targetPitch = midi.Note.Pitch + _glideConfig.PitchShift + pitchBend;
            _targetVelocity = midi.Note.Velocity;
            if (!_glideConfig.Enable || oldState != VoiceState.Active)
            {
                Pitch = _targetPitch;
                Delta = MathT.PitchToDelta(Pitch, _glideConfig.SampleRate);
                Velocity = (float)_targetVelocity;
                // EVENT CALL
                _pitchGlider.Reset(_targetPitch);
                // EVENT CALL
                _velocityGlider.Reset(_targetVelocity);
            }

            PolyPressure = midi.PolyPressure;
            result = midi.Length != 1 ? VoiceEvent.None
                : oldState == VoiceState.Inactive ? VoiceEvent.StartNote
                : VoiceEvent.RestartNote;
        }
        else
        {
            if (State == VoiceState.Active)
            {
                State = VoiceState.Release;
            }

            result = VoiceEvent.None;
        }

        if (_glideConfig.Enable)
        {
            var pitchDiff = _targetPitch - Pitch;
            if (pitchDiff != 0.0)
            {
                Pitch = Math.Abs(pitchDiff) < MathT.ExpThreshold
                    ? _targetPitch
                    : Pitch + pitchDiff * _glideConfig.GlideRate;
                Delta = MathT.PitchToDelta(Pitch, _glideConfig.SampleRate);

                if (result == VoiceEvent.None)
                    result = VoiceEvent.PitchChanged;
            }

            var velocityDiff = _targetVelocity - Velocity;
            if (velocityDiff != 0.0)
            {
                Velocity = Math.Abs(velocityDiff) < MathT.ExpThreshold
                    ? (float)_targetVelocity
                    : Velocity + (float)(velocityDiff * _glideConfig.GlideRate);
            }
        }

        return result;
    }
}
