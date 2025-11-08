using Tsumiki.Metadata;

namespace Tsumiki.Core;

internal enum VoiceState
{
    Inactive = 0,
    Active = 1,
    Release = 2,
}

[AudioTiming]
internal struct SynthVoice
{
    public VoiceState State;
    public float Velocity;
    public float PolyPressure;
    public double Delta;

    [AudioTiming]
    public bool Tick(in MidiVoice midi, double pitchBend, double sampleRate)
    {
        if (midi.Note.IsOn)
        {
            State = VoiceState.Active;
            Delta = MathT.PitchToDelta(midi.Note.Pitch + pitchBend, sampleRate);
            Velocity = midi.Note.Velocity;
            PolyPressure = midi.PolyPressure;

            return midi.Length == 1;
        }
        else
        {
            State = VoiceState.Release;
            return false;
        }
    }
}
