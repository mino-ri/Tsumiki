using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class SynthVoiceTest
{
    /// <summary>テスト用のInputユニットのモック</summary>
    private class TestInputUnit : IInputUnit
    {
        public int Bend { get; set; }
        public int Glide { get; set; }
        public int Octave { get; set; }
        public int Stack { get; set; }
        public StackMode StackMode { get; set; }
        public bool Polyphony { get; set; }
        public int StackDetune { get; set; }
        public float StackStereo { get; set; }
    }

    [Fact]
    public static void Tick_ノートオンでActiveになる()
    {
        var inputUnit = new TestInputUnit { Glide = -1 };
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote = new MidiNote(0, 60, 0.8f, 1);
        var midiVoice = new MidiVoice(in midiNote) { Length = 1 };

        var result = voice.Tick(in midiVoice, 0.0);

        Assert.Equal(VoiceState.Active, voice.State);
        Assert.Equal(0.8f, voice.Velocity);
        Assert.Equal(VoiceEvent.StartNote, result);
    }

    [Fact]
    public static void Tick_ノートオフでReleaseになる()
    {
        var inputUnit = new TestInputUnit { Glide = -1 };
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote = new MidiNote(0, 60, 0.8f, 1);
        var midiVoice = new MidiVoice(in midiNote) { Length = 1 };

        // まずノート・オン
        voice.Tick(in midiVoice, 0.0);
        Assert.Equal(VoiceState.Active, voice.State);

        // ノート・オフ
        var offNote = MidiNote.Off;
        var offVoice = new MidiVoice(in offNote) { Length = 2 };
        var result = voice.Tick(in offVoice, 0.0);

        Assert.Equal(VoiceState.Release, voice.State);
        Assert.Equal(VoiceEvent.None, result);
    }

    [Fact]
    public static void Tick_ピッチベンドが反映される()
    {
        var inputUnit = new TestInputUnit { Glide = -1 };
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote = new MidiNote(0, 60, 1.0f, 1);
        var midiVoice = new MidiVoice(in midiNote) { Length = 1 };

        var pitchBend = 2.0; // 2半音上げる
        voice.Tick(in midiVoice, pitchBend);

        // ピッチ60 + ベンド2 = 62 のデルタが計算されているはず
        var expectedDelta = MathT.PitchToDelta(62.0, 44100.0);
        Assert.Equal(expectedDelta, voice.Delta, 10);
    }

    [Fact]
    public static void Tick_ベロシティが正しく設定される()
    {
        var inputUnit = new TestInputUnit { Glide = -1 };
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var velocity = 0.75f;
        var midiNote = new MidiNote(0, 60, velocity, 1);
        var midiVoice = new MidiVoice(in midiNote) { Length = 1 };

        voice.Tick(in midiVoice, 0.0);

        Assert.Equal(velocity, voice.Velocity);
    }

    [Fact]
    public static void Tick_ポリプレッシャーが正しく設定される()
    {
        var inputUnit = new TestInputUnit { Glide = -1 };
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote = new MidiNote(0, 60, 1.0f, 1);
        var midiVoice = new MidiVoice(in midiNote) { Length = 1, PolyPressure = 0.6f };

        voice.Tick(in midiVoice, 0.0);

        Assert.Equal(0.6f, voice.PolyPressure);
    }

    [Fact]
    public static void Tick_Lengthが1の時にtrueを返す()
    {
        var inputUnit = new TestInputUnit { Glide = -1 };
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote = new MidiNote(0, 60, 1.0f, 1);
        var midiVoice1 = new MidiVoice(in midiNote) { Length = 1 };
        var midiVoice2 = new MidiVoice(in midiNote) { Length = 2 };
        var midiVoice0 = new MidiVoice(in midiNote) { Length = 0 };

        Assert.Equal(VoiceEvent.StartNote, voice.Tick(in midiVoice1, 0.0));
        Assert.Equal(VoiceEvent.None, voice.Tick(in midiVoice2, 0.0));
        Assert.Equal(VoiceEvent.None, voice.Tick(in midiVoice0, 0.0));
    }
}
