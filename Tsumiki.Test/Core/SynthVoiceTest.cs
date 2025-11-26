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

    [Fact]
    public static void Tick_リリース状態からのノートオンでRestartNoteを返す()
    {
        var inputUnit = new TestInputUnit { Glide = -1 };
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote = new MidiNote(0, 60, 1.0f, 1);
        var midiVoice = new MidiVoice(in midiNote) { Length = 1 };

        // 最初のノート・オン
        Assert.Equal(VoiceEvent.StartNote, voice.Tick(in midiVoice, 0.0));
        Assert.Equal(VoiceState.Active, voice.State);

        // ノート・オフでReleaseに遷移
        var offNote = MidiNote.Off;
        var offVoice = new MidiVoice(in offNote) { Length = 2 };
        voice.Tick(in offVoice, 0.0);
        Assert.Equal(VoiceState.Release, voice.State);

        // Release状態から再度ノート・オン（Length=1）
        var result = voice.Tick(in midiVoice, 0.0);
        Assert.Equal(VoiceEvent.RestartNote, result);
        Assert.Equal(VoiceState.Active, voice.State);
    }

    [Fact]
    public static void Tick_グライド有効時にPitchChangedを返す()
    {
        var inputUnit = new TestInputUnit { Glide = 30 }; // グライド有効
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote1 = new MidiNote(0, 60, 1.0f, 1);
        var midiNote2 = new MidiNote(0, 64, 1.0f, 2); // Lengthが1でない
        var midiVoice1 = new MidiVoice(in midiNote1) { Length = 1 };
        var midiVoice2 = new MidiVoice(in midiNote2) { Length = 2 };

        // 最初のノート・オン
        Assert.Equal(VoiceEvent.StartNote, voice.Tick(in midiVoice1, 0.0));

        // ピッチが変化したノート（Length != 1）でPitchChangedが返る
        var result = voice.Tick(in midiVoice2, 0.0);
        Assert.Equal(VoiceEvent.PitchChanged, result);
        Assert.Equal(60.0, voice.Pitch);
    }

    [Fact]
    public static void Tick_グライド無効時はPitchChangedを返さない()
    {
        var inputUnit = new TestInputUnit { Glide = -1 }; // グライド無効
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote1 = new MidiNote(0, 60, 1.0f, 1);
        var midiNote2 = new MidiNote(0, 64, 1.0f, 2); // Lengthが1でない
        var midiVoice1 = new MidiVoice(in midiNote1) { Length = 1 };
        var midiVoice2 = new MidiVoice(in midiNote2) { Length = 2 };

        // 最初のノート・オン
        Assert.Equal(VoiceEvent.StartNote, voice.Tick(in midiVoice1, 0.0));

        // グライド無効時はNoneが返る
        var result = voice.Tick(in midiVoice2, 0.0);
        Assert.Equal(VoiceEvent.None, result);
    }

    [Fact]
    public static void Tick_グライド有効時はDeltaが徐々に変化する()
    {
        var inputUnit = new TestInputUnit { Glide = 30 }; // グライド有効
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote1 = new MidiNote(0, 60, 1.0f, 1);
        var midiNote2 = new MidiNote(0, 72, 1.0f, 2); // 1オクターブ上
        var midiVoice1 = new MidiVoice(in midiNote1) { Length = 1 };
        var midiVoice2 = new MidiVoice(in midiNote2) { Length = 2 };

        // 最初のノート・オン（60）
        voice.Tick(in midiVoice1, 0.0);
        var initialDelta = voice.Delta;

        // ピッチ変更（72）
        voice.Tick(in midiVoice2, 0.0);
        var targetDelta = MathT.PitchToDelta(72.0, 44100.0);

        // グライドにより、Deltaは徐々に変化する
        var deltas = new double[100];
        for (var i = 0; i < 100; i++)
        {
            voice.Tick(in midiVoice2, 0.0);
            deltas[i] = voice.Delta;
        }

        // 初期値からターゲットに向かって変化していることを確認
        Assert.True(deltas[0] > initialDelta);
        Assert.True(deltas[0] < targetDelta);
        Assert.True(deltas[99] > deltas[0]);
    }

    [Fact]
    public static void Tick_グライド無効時はDeltaが即座に変化する()
    {
        var inputUnit = new TestInputUnit { Glide = -1 }; // グライド無効
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote1 = new MidiNote(0, 60, 1.0f, 1);
        var midiNote2 = new MidiNote(0, 72, 1.0f, 2); // 1オクターブ上
        var midiVoice1 = new MidiVoice(in midiNote1) { Length = 1 };
        var midiVoice2 = new MidiVoice(in midiNote2) { Length = 2 };

        // 最初のノート・オン（60）
        voice.Tick(in midiVoice1, 0.0);

        // ピッチ変更（72）
        voice.Tick(in midiVoice2, 0.0);
        var expectedDelta = MathT.PitchToDelta(72.0, 44100.0);

        // グライド無効なので、即座に変化する
        Assert.Equal(expectedDelta, voice.Delta, 10);
    }

    [Fact]
    public static void Tick_連続したノートオンでStateがActiveを維持する()
    {
        var inputUnit = new TestInputUnit { Glide = -1 };
        var glideConfig = new GliderConfig(inputUnit, 44100.0);
        var voice = new SynthVoice(glideConfig);
        var midiNote1 = new MidiNote(0, 60, 1.0f, 1);
        var midiNote2 = new MidiNote(0, 64, 1.0f, 2);
        var midiVoice1 = new MidiVoice(in midiNote1) { Length = 1 };
        var midiVoice2 = new MidiVoice(in midiNote2) { Length = 2 };

        // ノート・オン
        voice.Tick(in midiVoice1, 0.0);
        Assert.Equal(VoiceState.Active, voice.State);

        // 連続したノート
        voice.Tick(in midiVoice2, 0.0);
        Assert.Equal(VoiceState.Active, voice.State);

        // さらに続く
        voice.Tick(in midiVoice2, 0.0);
        Assert.Equal(VoiceState.Active, voice.State);
    }

    [Fact]
    public static void GliderConfig_Recalculate_Glide変更で再計算される()
    {
        var inputUnit = new TestInputUnit { Glide = 10 };
        var config = new GliderConfig(inputUnit, 44100.0);

        Assert.True(config.Enable);
        Assert.False(config.Polyphony);

        // Glide を変更
        inputUnit.Glide = 40;
        config.Recalculate(44100.0);

        // 値が更新されることを確認
        Assert.True(config.Enable);
        Assert.False(config.Polyphony);
    }

    [Fact]
    public static void GliderConfig_Recalculate_Glide負でPolyphonyがtrue()
    {
        var inputUnit = new TestInputUnit { Glide = 10 };
        var config = new GliderConfig(inputUnit, 44100.0);

        Assert.True(config.Enable);
        Assert.False(config.Polyphony);

        // Glide を負の値に変更
        inputUnit.Glide = -1;
        config.Recalculate(44100.0);

        // Polyphony が true、Enable が false になる
        Assert.False(config.Enable);
        Assert.True(config.Polyphony);
    }

    [Fact]
    public static void GliderConfig_Glide値によるEnable切り替え()
    {
        // Glide > 0 の場合
        var inputUnit1 = new TestInputUnit { Glide = 30 };
        var config1 = new GliderConfig(inputUnit1, 44100.0);
        Assert.True(config1.Enable);
        Assert.False(config1.Polyphony);

        // Glide = 0 の場合
        var inputUnit2 = new TestInputUnit { Glide = 0 };
        var config2 = new GliderConfig(inputUnit2, 44100.0);
        Assert.False(config2.Enable);
        Assert.False(config2.Polyphony);

        // Glide < 0 の場合
        var inputUnit3 = new TestInputUnit { Glide = -1 };
        var config3 = new GliderConfig(inputUnit3, 44100.0);
        Assert.False(config3.Enable);
        Assert.True(config3.Polyphony);
    }

    [Fact]
    public static void GliderConfig_Recalculate_サンプルレート変更で再計算される()
    {
        var inputUnit = new TestInputUnit { Glide = 30 };
        var config = new GliderConfig(inputUnit, 44100.0);

        Assert.Equal(44100.0, config.SampleRate);

        // サンプルレートを変更
        config.Recalculate(48000.0);

        // SampleRate が更新される
        Assert.Equal(48000.0, config.SampleRate);
    }

    [Fact]
    public static void GliderConfig_Recalculate_変更なしではスキップされる()
    {
        var inputUnit = new TestInputUnit { Glide = 30 };
        var config = new GliderConfig(inputUnit, 44100.0);

        var oldSampleRate = config.SampleRate;
        var oldEnable = config.Enable;

        // 同じ値で再計算
        config.Recalculate(44100.0);

        // 値が変わっていないことを確認
        Assert.Equal(oldSampleRate, config.SampleRate);
        Assert.Equal(oldEnable, config.Enable);
    }
}
