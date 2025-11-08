using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class MidiNoteTest
{
    [Fact]
    public static void Constructor_プロパティを正しく設定する()
    {
        var note = new MidiNote(1, 60, 0.8f, 100);

        Assert.Equal(1, note.Channel);
        Assert.Equal(60, note.Pitch);
        Assert.Equal(0.8f, note.Velocity);
        Assert.Equal(100, note.NoteId);
    }

    [Fact]
    public static void Off_正しい値を持つ()
    {
        var noteOff = MidiNote.Off;

        Assert.Equal(0, noteOff.Channel);
        Assert.Equal(0, noteOff.Pitch);
        Assert.Equal(MidiNote.OffVelocity, noteOff.Velocity);
        Assert.Equal(MidiNote.GlobalNoteId, noteOff.NoteId);
        Assert.False(noteOff.IsOn);
    }

    [Fact]
    public static void IsOn_有効なノートでTrueを返す()
    {
        var note = new MidiNote(0, 60, 0.5f, 1);
        Assert.True(note.IsOn);
    }

    [Fact]
    public static void IsOn_ベロシティ0でTrueを返す()
    {
        var note = new MidiNote(0, 60, 0f, 1);
        Assert.True(note.IsOn);
    }

    [Fact]
    public static void IsOn_OffVelocityでFalseを返す()
    {
        var note = new MidiNote(0, 60, MidiNote.OffVelocity, 1);
        Assert.False(note.IsOn);
    }

    [Fact]
    public static void IsSame_同じNoteIdの場合Trueを返す()
    {
        var note1 = new MidiNote(0, 60, 0.8f, 100);
        var note2 = new MidiNote(0, 62, 0.5f, 100);

        Assert.True(note1.IsSame(in note2));
    }

    [Fact]
    public static void IsSame_異なるNoteIdの場合Falseを返す()
    {
        var note1 = new MidiNote(0, 60, 0.8f, 100);
        var note2 = new MidiNote(0, 60, 0.8f, 101);

        Assert.False(note1.IsSame(in note2));
    }

    [Fact]
    public static void IsSame_NoteIdが負で同じピッチの場合Trueを返す()
    {
        var note1 = new MidiNote(0, 60, 0.8f, -1);
        var note2 = new MidiNote(0, 60, 0.5f, -1);

        Assert.True(note1.IsSame(in note2));
    }

    [Fact]
    public static void IsSame_NoteIdが負で異なるピッチの場合Falseを返す()
    {
        var note1 = new MidiNote(0, 60, 0.8f, -1);
        var note2 = new MidiNote(0, 62, 0.5f, -1);

        Assert.False(note1.IsSame(in note2));
    }

    [Fact]
    public static void IsSame_異なるチャンネルの場合Falseを返す()
    {
        var note1 = new MidiNote(0, 60, 0.8f, 100);
        var note2 = new MidiNote(1, 60, 0.8f, 100);

        Assert.False(note1.IsSame(in note2));
    }

    [Fact]
    public static void IsSame_片方のNoteIdが負で同じピッチの場合Trueを返す()
    {
        var note1 = new MidiNote(0, 60, 0.8f, 100);
        var note2 = new MidiNote(0, 60, 0.5f, -1);

        Assert.True(note1.IsSame(in note2));
    }

    [Fact]
    public static void IsSame_片方のNoteIdが負で異なるピッチの場合Falseを返す()
    {
        var note1 = new MidiNote(0, 60, 0.8f, 100);
        var note2 = new MidiNote(0, 62, 0.5f, -1);

        Assert.False(note1.IsSame(in note2));
    }
}
