using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class ProcessorTest
{
    [Fact]
    public static void OnActive_アクティブ化するとIsActiveがtrueになる()
    {
        var processor = new Processor();

        Assert.False(processor.IsActive);

        processor.OnActive(true, new TsumikiModel(), 44100);

        Assert.True(processor.IsActive);
    }

    [Fact]
    public static void OnActive_非アクティブ化するとIsActiveがfalseになる()
    {
        var processor = new Processor();

        processor.OnActive(true, new TsumikiModel(), 44100);
        Assert.True(processor.IsActive);

        processor.OnActive(false, new TsumikiModel(), 44100);
        Assert.False(processor.IsActive);
    }

    [Fact]
    public static void ProcessMain_適切なパラメータを入力すれば音が出力される()
    {
        var processor = new Processor();
        var model = new TsumikiModel();

        // プロセッサをアクティブ化
        processor.OnActive(true, new TsumikiModel(), 44100);

        // サンプルレートとパラメータを設定
        const double sampleRate = 44100.0;

        // モデルパラメータを設定（デフォルト値から一部変更）
        model.Master = 1.0f;
        model.PitchBend = 0.5f; // 中央値（ベンドなし）
        model.A1.Pitch = 1.0;
        model.A1.Level = 1f;

        Assert.Equal(1.0f, model.Master);

        // Recalculateを呼び出してパラメータを反映
        processor.Recalculate(model, sampleRate);

        // ノートオンを予約（ピッチ60 = C4, ベロシティ0.8）
        var noteOn = new MidiNote(0, 60, 0.8f, 1);
        var reservation = new MidiEventReservation<MidiNote>(in noteOn, 0);
        processor.ReserveNote(in reservation);

        // 出力バッファを準備（256サンプル）
        const int sampleCount = 4096;
        var leftOutput = new float[sampleCount];
        var rightOutput = new float[sampleCount];

        // 音声処理を実行
        processor.ProcessMain(model, sampleRate, sampleCount, leftOutput, rightOutput);

        // 出力に音が含まれているか確認（0以外の値が存在する）
        var maxLevel = 0f;
        for (var i = 0; i < sampleCount; i++)
        {
            maxLevel = MathF.Max(maxLevel, MathF.Max(MathF.Abs(leftOutput[i]), MathF.Abs(rightOutput[i])));
        }

        Assert.True(0.99f <= maxLevel, $"最大音量で出力でされる: {maxLevel}");
        Assert.True(1.01f >= maxLevel, $"音量が1を超えない: {maxLevel}");
    }

    [Fact]
    public static void ProcessMain_ノートオンなしでは音が出力されない()
    {
        var processor = new Processor();
        var model = new TsumikiModel();

        processor.OnActive(true, new TsumikiModel(), 44100);

        const double sampleRate = 44100.0;
        processor.Recalculate(model, sampleRate);

        // ノートオンを予約せずに処理

        const int sampleCount = 256;
        var leftOutput = new float[sampleCount];
        var rightOutput = new float[sampleCount];

        processor.ProcessMain(model, sampleRate, sampleCount, leftOutput, rightOutput);

        // 出力がすべて0であることを確認
        for (var i = 0; i < sampleCount; i++)
        {
            Assert.Equal(0f, leftOutput[i]);
            Assert.Equal(0f, rightOutput[i]);
        }
    }


    [Fact]
    public static void ProcessMain_複数ノートで音が出力される()
    {
        var processor = new Processor();
        var model = new TsumikiModel();

        processor.OnActive(true, new TsumikiModel(), 44100);

        const double sampleRate = 44100.0;
        model.Master = 0.5f;
        processor.Recalculate(model, sampleRate);

        // 複数のノートオンを予約
        var note1 = new MidiNote(0, 60, 0.8f, 1);
        processor.ReserveNote(new MidiEventReservation<MidiNote>(in note1, 0));

        var note2 = new MidiNote(0, 64, 0.7f, 2);
        processor.ReserveNote(new MidiEventReservation<MidiNote>(in note2, 0));

        var note3 = new MidiNote(0, 67, 0.6f, 3);
        processor.ReserveNote(new MidiEventReservation<MidiNote>(in note3, 0));

        const int sampleCount = 256;
        var leftOutput = new float[sampleCount];
        var rightOutput = new float[sampleCount];

        processor.ProcessMain(model, sampleRate, sampleCount, leftOutput, rightOutput);

        // 音が出力されることを確認
        var hasNonZeroOutput = false;
        for (var i = 0; i < sampleCount; i++)
        {
            if (leftOutput[i] != 0f || rightOutput[i] != 0f)
            {
                hasNonZeroOutput = true;
                break;
            }
        }

        Assert.True(hasNonZeroOutput, "複数ノートの音声出力に0以外の値が含まれる必要がある");
    }

    [Fact]
    public static void ProcessMain_ノートオフ後はエンベロープが減衰する()
    {
        var processor = new Processor();
        var model = new TsumikiModel();

        processor.OnActive(true, new TsumikiModel(), 44100);

        const double sampleRate = 44100.0;
        model.Master = 0.5f;
        processor.Recalculate(model, sampleRate);

        // ノートオン
        var noteOn = new MidiNote(0, 60, 0.8f, 1);
        processor.ReserveNote(new MidiEventReservation<MidiNote>(in noteOn, 0));

        // 最初のバッチを処理（ノートオン状態）
        const int sampleCount = 256;
        var leftOutput1 = new float[sampleCount];
        var rightOutput1 = new float[sampleCount];
        processor.ProcessMain(model, sampleRate, sampleCount, leftOutput1, rightOutput1);

        // 音が出ていることを確認
        var hasNonZero1 = leftOutput1.Any(x => x != 0f);
        Assert.True(hasNonZero1, "ノートオン時に音が出力される必要がある");

        // ノートオフ
        var noteOff = new MidiNote(0, 60, MidiNote.OffVelocity, 1);
        processor.ReserveNote(new MidiEventReservation<MidiNote>(in noteOff, 0));

        // 2回目のバッチを処理（ノートオフ後、エンベロープがリリース段階）
        var leftOutput2 = new float[sampleCount];
        var rightOutput2 = new float[sampleCount];
        processor.ProcessMain(model, sampleRate, sampleCount, leftOutput2, rightOutput2);

        // まだ音が出ている（リリース中）
        var hasNonZero2 = leftOutput2.Any(x => x != 0f);
        Assert.True(hasNonZero2, "ノートオフ後のリリース段階でも音が出力される必要がある");

        // 十分な時間が経過すると音が止まる（エンベロープが完全に減衰）
        // 複数バッチ処理して確認
        var leftOutput3 = new float[sampleCount];
        var rightOutput3 = new float[sampleCount];
        for (var i = 0; i < 100; i++)
        {
            processor.ProcessMain(model, sampleRate, sampleCount, leftOutput3, rightOutput3);
        }

        // 最終的には無音になる
        var allZero = true;
        for (var i = 0; i < sampleCount; i++)
        {
            if (leftOutput3[i] != 0f || rightOutput3[i] != 0f)
            {
                allZero = false;
                break;
            }
        }

        Assert.True(allZero, "十分な時間経過後、エンベロープが完全に減衰して無音になる必要がある");
    }

}
