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
    private double _targetDelta;
    private double _targetVelocity;
    private LowPassFilterD _pitchGlider;
    private LowPassFilterD _velocityGlider;

    [AudioTiming]
    public VoiceEvent Tick(in MidiVoice midi, double pitchBend)
    {
        if (_glideConfig.Enable)
        {
            Delta = _pitchGlider.TickAndRender(in _glideConfig.Filter, _targetDelta);
            Velocity = (float)_velocityGlider.TickAndRender(in _glideConfig.Filter, _targetVelocity);
        }

        if (midi.Note.IsOn)
        {
            var oldState = State;
            State = VoiceState.Active;
            Pitch = midi.Note.Pitch + pitchBend;
            _targetDelta = MathT.PitchToDelta(Pitch, _glideConfig.SampleRate);
            _targetVelocity = midi.Note.Velocity;
            if (!_glideConfig.Enable || oldState != VoiceState.Active)
            {
                Delta = _targetDelta;
                Velocity = (float)_targetVelocity;
                // EVENT CALL
                _pitchGlider.Reset(_targetDelta);
                // EVENT CALL
                _velocityGlider.Reset(_targetVelocity);
            }

            PolyPressure = midi.PolyPressure;

            return midi.Length != 1 ? (_glideConfig.Enable ? VoiceEvent.PitchChanged : VoiceEvent.None)
                : oldState == VoiceState.Inactive ? VoiceEvent.StartNote
                : VoiceEvent.RestartNote;
        }
        else
        {
            State = VoiceState.Release;
            return VoiceEvent.None;
        }
    }
}
