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
}

[AudioTiming]
internal struct SynthVoice
{
    public VoiceState State;
    public float Velocity;
    public float PolyPressure;
    public double Pitch;
    public double Delta;

    [AudioTiming]
    public VoiceEvent Tick(in MidiVoice midi, double pitchBend, double sampleRate)
    {
        if (midi.Note.IsOn)
        {
            var oldState = State;
            State = VoiceState.Active;
            Pitch = midi.Note.Pitch + pitchBend;
            Delta = MathT.PitchToDelta(Pitch, sampleRate);
            Velocity = midi.Note.Velocity;
            PolyPressure = midi.PolyPressure;

            return midi.Length != 1 ? VoiceEvent.None
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
