using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class MidiNoteReservationTest
{
    [Fact]
    public static void Constructor_プロパティを正しく設定する()
    {
        var input = new MidiNote(1, 60, 0.8f, 100);
        var reservation = new MidiNoteReservation(in input, 1000);

        Assert.Equal(input.Channel, reservation.Note.Channel);
        Assert.Equal(input.Pitch, reservation.Note.Pitch);
        Assert.Equal(input.Velocity, reservation.Note.Velocity);
        Assert.Equal(input.NoteId, reservation.Note.NoteId);
        Assert.Equal(1000, reservation.SampleOffset);
    }

    [Fact]
    public static void SampleOffset_変更可能である()
    {
        var input = new MidiNote(0, 60, 0.5f, 1);
        var reservation = new MidiNoteReservation(in input, 1000);

        reservation.SampleOffset = 500;
        Assert.Equal(500, reservation.SampleOffset);
    }
}
