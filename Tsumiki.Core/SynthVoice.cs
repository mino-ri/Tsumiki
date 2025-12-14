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
[method: InitTiming]
internal struct SynthVoice(GliderConfig glideConfig)
{
    private readonly GliderConfig _glideConfig = glideConfig;
    public VoiceState State;
    public float Velocity;
    public float PolyPressure;
    public double Pitch;
    public double Delta;
    private Glide _pitchGlide = new(glideConfig);
    private Glide _velocityGlide = new(glideConfig);

    [AudioTiming]
    public VoiceEvent Tick(in MidiVoice midi, double pitchBend)
    {
        VoiceEvent result;
        if (midi.Note.IsOn)
        {
            var oldState = State;
            State = VoiceState.Active;
            var targetPitch = midi.Note.Pitch + _glideConfig.PitchShift + pitchBend;
            var targetVelocity = midi.Note.Velocity;
            if (!_glideConfig.Enable || oldState != VoiceState.Active)
            {
                Pitch = targetPitch;
                Delta = MathT.PitchToDelta(Pitch, _glideConfig.SampleRate);
                Velocity = targetVelocity;
                // EVENT CALL
                _pitchGlide.Reset(targetPitch);
                // EVENT CALL
                _velocityGlide.Reset(targetVelocity);
            }
            else
            {
                _pitchGlide.SetTarget(targetPitch);
                _velocityGlide.SetTarget(targetVelocity);
            }

            PolyPressure = midi.PolyPressure;
            result = oldState switch
            {
                VoiceState.Inactive => VoiceEvent.StartNote,
                VoiceState.Release => VoiceEvent.RestartNote,
                _ => VoiceEvent.None,
            };
        }
        else
        {
            if (State == VoiceState.Active)
            {
                State = VoiceState.Release;
            }

            result = VoiceEvent.None;
        }

        if (!_glideConfig.Enable)
        {
            return result;
        }

        Velocity = (float)_velocityGlide.TickAndRender();
        var newPitch = _pitchGlide.TickAndRender();
        if (Pitch != newPitch)
        {
            Pitch = newPitch;
            Delta = MathT.PitchToDelta(Pitch, _glideConfig.SampleRate);
            if (result == VoiceEvent.None)
            {
                return VoiceEvent.PitchChanged;
            }
        }

        return result;
    }
}
