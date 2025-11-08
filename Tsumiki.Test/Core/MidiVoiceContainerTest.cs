using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class MidiVoiceContainerTest
{
    [Fact]
    public static void Constructor_配列を初期化する()
    {
        var container = new MidiVoiceContainer(4);

        Assert.NotNull(container.Reservations);
        Assert.NotNull(container.PressureReservations);
        Assert.NotNull(container.Voices);
        Assert.Equal(4, container.Reservations.Length);
        Assert.Equal(4, container.PressureReservations.Length);
        Assert.Equal(4, container.Voices.Length);
    }

    [Fact]
    public static void Constructor_Offで初期化する()
    {
        var container = new MidiVoiceContainer(2);

        // すべての予約は Off と -1 オフセットで初期化される
        for (var i = 0; i < container.Reservations.Length; i++)
        {
            Assert.False(container.Reservations[i].Event.IsOn);
            Assert.Equal(-1, container.Reservations[i].SampleOffset);
        }

        // すべてのプレッシャー予約はデフォルト値で初期化される
        for (var i = 0; i < container.PressureReservations.Length; i++)
        {
            Assert.Equal(-1, container.PressureReservations[i].SampleOffset);
        }

        // すべてのボイスは Off で初期化される
        for (var i = 0; i < container.Voices.Length; i++)
        {
            Assert.False(container.Voices[i].Note.IsOn);
            Assert.Equal(0, container.Voices[i].Length);
            Assert.Equal(0f, container.Voices[i].PolyPressure);
        }
    }

    [Fact]
    public static void Reserve_オフセット0の場合は即座に実行する()
    {
        var container = new MidiVoiceContainer(2);
        var input = new MidiNote(0, 60, 0.8f, 1);
        var reservation = new MidiEventReservation<MidiNote>(in input, 0);

        container.Reserve(in reservation);

        // 即座に実行される（ボイスに割り当てられる）
        var foundVoice = false;
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.Pitch == 60)
            {
                foundVoice = true;
                Assert.Equal(0.8f, container.Voices[i].Note.Velocity);
                break;
            }
        }
        Assert.True(foundVoice, "ボイスが即座に割り当てられる必要がある");
    }

    [Fact]
    public static void Reserve_オフセットが正の場合は予約を保存する()
    {
        var container = new MidiVoiceContainer(2);
        var input = new MidiNote(0, 60, 0.8f, 1);
        var reservation = new MidiEventReservation<MidiNote>(in input, 100);

        container.Reserve(in reservation);

        // 予約に保存される
        var foundReservation = false;
        for (var i = 0; i < container.Reservations.Length; i++)
        {
            if (container.Reservations[i].Event.Pitch == 60)
            {
                foundReservation = true;
                Assert.Equal(100, container.Reservations[i].SampleOffset);
                break;
            }
        }
        Assert.True(foundReservation, "予約が保存される必要がある");
    }

    [Fact]
    public static void Reserve_満杯の場合は最も古い予約を上書きする()
    {
        var container = new MidiVoiceContainer(2);

        // すべての予約スロットを埋める
        var input1 = new MidiNote(0, 60, 0.8f, 1);
        var reservation1 = new MidiEventReservation<MidiNote>(in input1, 100);
        container.Reserve(in reservation1);

        var input2 = new MidiNote(0, 62, 0.8f, 2);
        var reservation2 = new MidiEventReservation<MidiNote>(in input2, 200);
        container.Reserve(in reservation2);

        // 3つ目の予約を追加（オフセット200の最も古いものが上書きされる）
        var input3 = new MidiNote(0, 64, 0.8f, 3);
        var reservation3 = new MidiEventReservation<MidiNote>(in input3, 50);
        container.Reserve(in reservation3);

        // ピッチ64が存在し、ピッチ62が上書きされたことを確認
        var foundPitch64 = false;
        var foundPitch60 = false;
        var foundPitch62 = false;

        for (var i = 0; i < container.Reservations.Length; i++)
        {
            if (container.Reservations[i].Event.Pitch == 64) foundPitch64 = true;
            if (container.Reservations[i].Event.Pitch == 60) foundPitch60 = true;
            if (container.Reservations[i].Event.Pitch == 62) foundPitch62 = true;
        }

        Assert.True(foundPitch64, "新しい予約が追加される必要がある");
        Assert.True(foundPitch60, "より小さいオフセットの予約は残る必要がある");
        Assert.False(foundPitch62, "最も古い予約が上書きされる必要がある");
    }

    [Fact]
    public static void Tick_ボイスの長さを増加させる()
    {
        var container = new MidiVoiceContainer(2);
        var input = new MidiNote(0, 60, 0.8f, 1);
        var reservation = new MidiEventReservation<MidiNote>(in input, 0);

        container.Reserve(in reservation);

        // 初期の長さを取得
        var initialLength = container.Voices[0].Length;

        // 1回Tickする
        container.Tick();

        // 長さが増加している
        Assert.Equal(initialLength + 1, container.Voices[0].Length);
    }

    [Fact]
    public static void Tick_予約のオフセットを減少させる()
    {
        var container = new MidiVoiceContainer(2);
        var input = new MidiNote(0, 60, 0.8f, 1);
        var reservation = new MidiEventReservation<MidiNote>(in input, 10);

        container.Reserve(in reservation);

        // 1回Tickする
        container.Tick();

        // 予約を見つけてオフセットを確認
        for (var i = 0; i < container.Reservations.Length; i++)
        {
            if (container.Reservations[i].Event.Pitch == 60)
            {
                Assert.Equal(9, container.Reservations[i].SampleOffset);
                return;
            }
        }
        Assert.Fail("予約が見つからない");
    }

    [Fact]
    public static void Tick_オフセット0になったら予約を実行する()
    {
        var container = new MidiVoiceContainer(2);
        var input = new MidiNote(0, 60, 0.8f, 1);
        var reservation = new MidiEventReservation<MidiNote>(in input, 1);

        container.Reserve(in reservation);

        // 2回Tickする - 2回目で予約が実行される
        container.Tick();
        container.Tick();

        // ボイスが作成されたことを確認
        var foundVoice = false;
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.Pitch == 60 && container.Voices[i].Note.IsOn)
            {
                foundVoice = true;
                break;
            }
        }
        Assert.True(foundVoice, "Tick後にボイスが作成される必要がある");
    }

    [Fact]
    public static void VoiceAllocation_非アクティブなボイスを優先する()
    {
        var container = new MidiVoiceContainer(2);

        // 最初のボイスをアクティブにする
        var input1 = new MidiNote(0, 60, 0.8f, 1);
        var reservation1 = new MidiEventReservation<MidiNote>(in input1, 0);
        container.Reserve(in reservation1);

        // もう1つノートを追加
        var input2 = new MidiNote(0, 62, 0.8f, 2);
        var reservation2 = new MidiEventReservation<MidiNote>(in input2, 0);
        container.Reserve(in reservation2);

        // 両方のボイスがアクティブになっている
        var activeCount = 0;
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.IsOn)
            {
                activeCount++;
            }
        }
        Assert.Equal(2, activeCount);
    }

    [Fact]
    public static void VoiceAllocation_全て使用中の場合は最も古いボイスを奪う()
    {
        var container = new MidiVoiceContainer(2);

        // すべてのボイスを埋める
        var input1 = new MidiNote(0, 60, 0.8f, 1);
        container.Reserve(new MidiEventReservation<MidiNote>(in input1, 0));

        var input2 = new MidiNote(0, 62, 0.8f, 2);
        container.Reserve(new MidiEventReservation<MidiNote>(in input2, 0));

        // Tickして最初のボイスを古くする
        container.Tick();
        container.Tick();

        // 3つ目のノートを追加（最も古いボイスを奪う）
        var input3 = new MidiNote(0, 64, 0.8f, 3);
        container.Reserve(new MidiEventReservation<MidiNote>(in input3, 0));

        // ピッチ64が存在することを確認
        var foundPitch64 = false;
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.Pitch == 64 && container.Voices[i].Note.IsOn)
            {
                foundPitch64 = true;
                break;
            }
        }
        Assert.True(foundPitch64, "新しいボイスが最も古いボイスを奪う必要がある");
    }

    [Fact]
    public static void NoteOff_NoteIdが一致するボイスをOffにする()
    {
        var container = new MidiVoiceContainer(3);

        // 3つのノートをオンにする
        var note1 = new MidiNote(0, 60, 0.8f, 100);
        container.Reserve(new MidiEventReservation<MidiNote>(in note1, 0));

        var note2 = new MidiNote(0, 62, 0.7f, 101);
        container.Reserve(new MidiEventReservation<MidiNote>(in note2, 0));

        var note3 = new MidiNote(0, 64, 0.6f, 102);
        container.Reserve(new MidiEventReservation<MidiNote>(in note3, 0));

        // note2をオフにする
        var noteOff = new MidiNote(0, 62, MidiNote.OffVelocity, 101);
        container.Reserve(new MidiEventReservation<MidiNote>(in noteOff, 0));

        // note1とnote3はまだオン、note2はオフになっていることを確認
        var onCount = 0;
        var foundNote1 = false;
        var foundNote3 = false;

        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.IsOn)
            {
                onCount++;
                if (container.Voices[i].Note.NoteId == 100) foundNote1 = true;
                if (container.Voices[i].Note.NoteId == 102) foundNote3 = true;
            }
        }

        Assert.Equal(2, onCount);
        Assert.True(foundNote1, "Note1はまだオンである必要がある");
        Assert.True(foundNote3, "Note3はまだオンである必要がある");
    }

    [Fact]
    public static void NoteOff_NoteIdが負の場合はピッチで判定する()
    {
        var container = new MidiVoiceContainer(3);

        // 3つのノートをオンにする（NoteId = -1）
        var note1 = new MidiNote(0, 60, 0.8f, -1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note1, 0));

        var note2 = new MidiNote(0, 62, 0.7f, -1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note2, 0));

        var note3 = new MidiNote(0, 64, 0.6f, -1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note3, 0));

        // ピッチ62をオフにする
        var noteOff = new MidiNote(0, 62, MidiNote.OffVelocity, MidiNote.GlobalNoteId);
        container.Reserve(new MidiEventReservation<MidiNote>(in noteOff, 0));

        // ピッチ60と64はまだオン、ピッチ62はオフになっていることを確認
        var onCount = 0;
        var foundPitch60 = false;
        var foundPitch64 = false;

        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.IsOn)
            {
                onCount++;
                if (container.Voices[i].Note.Pitch == 60) foundPitch60 = true;
                if (container.Voices[i].Note.Pitch == 64) foundPitch64 = true;
            }
        }

        Assert.Equal(2, onCount);
        Assert.True(foundPitch60, "ピッチ60はまだオンである必要がある");
        Assert.True(foundPitch64, "ピッチ64はまだオンである必要がある");
    }

    [Fact]
    public static void NoteOff_異なるチャンネルのノートには影響しない()
    {
        var container = new MidiVoiceContainer(2);

        // チャンネル0でノートオン
        var note1 = new MidiNote(0, 60, 0.8f, 100);
        container.Reserve(new MidiEventReservation<MidiNote>(in note1, 0));

        // チャンネル1でノートオン
        var note2 = new MidiNote(1, 60, 0.8f, 100);
        container.Reserve(new MidiEventReservation<MidiNote>(in note2, 0));

        // チャンネル0のピッチ60をオフにする
        var noteOff = new MidiNote(0, 60, MidiNote.OffVelocity, 100);
        container.Reserve(new MidiEventReservation<MidiNote>(in noteOff, 0));

        // チャンネル0のノートはオフ、チャンネル1のノートはまだオンであることを確認
        var foundChannel1Note = false;

        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.IsOn && container.Voices[i].Note.Channel == 1)
            {
                foundChannel1Note = true;
            }
        }

        Assert.True(foundChannel1Note, "チャンネル1のノートはまだオンである必要がある");
    }

    [Fact]
    public static void Reserve_PolyPressureをオフセット0の場合は即座に実行する()
    {
        var container = new MidiVoiceContainer(2);

        // まずノートをオンにする
        var note = new MidiNote(0, 60, 0.8f, 1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note, 0));

        // プレッシャーを即座に適用
        var pressure = new MidiPolyPressure(0, 60, 0.7f, 1);
        var reservation = new MidiEventReservation<MidiPolyPressure>(in pressure, 0);

        container.Reserve(in reservation);

        // ボイスにプレッシャーが設定されたことを確認
        var foundVoice = false;
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.Pitch == 60)
            {
                foundVoice = true;
                Assert.Equal(0.7f, container.Voices[i].PolyPressure);
                break;
            }
        }
        Assert.True(foundVoice, "プレッシャーが即座に適用される必要がある");
    }

    [Fact]
    public static void Reserve_PolyPressureをオフセットが正の場合は予約を保存する()
    {
        var container = new MidiVoiceContainer(2);
        var pressure = new MidiPolyPressure(0, 60, 0.7f, 1);
        var reservation = new MidiEventReservation<MidiPolyPressure>(in pressure, 100);

        container.Reserve(in reservation);

        // 予約に保存される
        var foundReservation = false;
        for (var i = 0; i < container.PressureReservations.Length; i++)
        {
            if (container.PressureReservations[i].Event.Pitch == 60)
            {
                foundReservation = true;
                Assert.Equal(100, container.PressureReservations[i].SampleOffset);
                Assert.Equal(0.7f, container.PressureReservations[i].Event.Pressure);
                break;
            }
        }
        Assert.True(foundReservation, "プレッシャー予約が保存される必要がある");
    }

    [Fact]
    public static void Reserve_PolyPressureが満杯の場合は最も古い予約を上書きする()
    {
        var container = new MidiVoiceContainer(2);

        // すべての予約スロットを埋める
        var pressure1 = new MidiPolyPressure(0, 60, 0.5f, 1);
        var reservation1 = new MidiEventReservation<MidiPolyPressure>(in pressure1, 100);
        container.Reserve(in reservation1);

        var pressure2 = new MidiPolyPressure(0, 62, 0.6f, 2);
        var reservation2 = new MidiEventReservation<MidiPolyPressure>(in pressure2, 200);
        container.Reserve(in reservation2);

        // 3つ目の予約を追加（オフセット200の最も古いものが上書きされる）
        var pressure3 = new MidiPolyPressure(0, 64, 0.7f, 3);
        var reservation3 = new MidiEventReservation<MidiPolyPressure>(in pressure3, 50);
        container.Reserve(in reservation3);

        // ピッチ64が存在し、ピッチ62が上書きされたことを確認
        var foundPitch64 = false;
        var foundPitch60 = false;
        var foundPitch62 = false;

        for (var i = 0; i < container.PressureReservations.Length; i++)
        {
            if (container.PressureReservations[i].Event.Pitch == 64) foundPitch64 = true;
            if (container.PressureReservations[i].Event.Pitch == 60) foundPitch60 = true;
            if (container.PressureReservations[i].Event.Pitch == 62) foundPitch62 = true;
        }

        Assert.True(foundPitch64, "新しいプレッシャー予約が追加される必要がある");
        Assert.True(foundPitch60, "より小さいオフセットの予約は残る必要がある");
        Assert.False(foundPitch62, "最も古いプレッシャー予約が上書きされる必要がある");
    }

    [Fact]
    public static void Tick_PolyPressure予約のオフセットを減少させる()
    {
        var container = new MidiVoiceContainer(2);
        var pressure = new MidiPolyPressure(0, 60, 0.7f, 1);
        var reservation = new MidiEventReservation<MidiPolyPressure>(in pressure, 10);

        container.Reserve(in reservation);

        // 1回Tickする
        container.Tick();

        // 予約を見つけてオフセットを確認
        for (var i = 0; i < container.PressureReservations.Length; i++)
        {
            if (container.PressureReservations[i].Event.Pitch == 60)
            {
                Assert.Equal(9, container.PressureReservations[i].SampleOffset);
                return;
            }
        }
        Assert.Fail("プレッシャー予約が見つからない");
    }

    [Fact]
    public static void Tick_PolyPressure予約がオフセット0になったら実行する()
    {
        var container = new MidiVoiceContainer(2);

        // まずノートをオンにする
        var note = new MidiNote(0, 60, 0.8f, 1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note, 0));

        // プレッシャーを予約する（オフセット1）
        var pressure = new MidiPolyPressure(0, 60, 0.7f, 1);
        var reservation = new MidiEventReservation<MidiPolyPressure>(in pressure, 1);
        container.Reserve(in reservation);

        // 初期状態ではプレッシャーは0
        var initialPressure = 0f;
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.Pitch == 60)
            {
                initialPressure = container.Voices[i].PolyPressure;
                break;
            }
        }
        Assert.Equal(0f, initialPressure);

        // 2回Tickする - 2回目で予約が実行される
        container.Tick();
        container.Tick();

        // プレッシャーが適用されたことを確認
        var foundVoice = false;
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.Pitch == 60)
            {
                foundVoice = true;
                Assert.Equal(0.7f, container.Voices[i].PolyPressure);
                break;
            }
        }
        Assert.True(foundVoice, "Tick後にプレッシャーが適用される必要がある");
    }

    [Fact]
    public static void ProcessPolyPressure_NoteIdが一致するボイスにプレッシャーを適用する()
    {
        var container = new MidiVoiceContainer(3);

        // 3つのノートをオンにする
        var note1 = new MidiNote(0, 60, 0.8f, 100);
        container.Reserve(new MidiEventReservation<MidiNote>(in note1, 0));

        var note2 = new MidiNote(0, 62, 0.7f, 101);
        container.Reserve(new MidiEventReservation<MidiNote>(in note2, 0));

        var note3 = new MidiNote(0, 64, 0.6f, 102);
        container.Reserve(new MidiEventReservation<MidiNote>(in note3, 0));

        // note2にプレッシャーを適用する
        var pressure = new MidiPolyPressure(0, 62, 0.9f, 101);
        container.Reserve(new MidiEventReservation<MidiPolyPressure>(in pressure, 0));

        // note2のみにプレッシャーが適用されていることを確認
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.NoteId == 100)
            {
                Assert.Equal(0f, container.Voices[i].PolyPressure);
            }
            else if (container.Voices[i].Note.NoteId == 101)
            {
                Assert.Equal(0.9f, container.Voices[i].PolyPressure);
            }
            else if (container.Voices[i].Note.NoteId == 102)
            {
                Assert.Equal(0f, container.Voices[i].PolyPressure);
            }
        }
    }

    [Fact]
    public static void ProcessPolyPressure_NoteIdが負の場合はピッチで判定する()
    {
        var container = new MidiVoiceContainer(3);

        // 3つのノートをオンにする（NoteId = -1）
        var note1 = new MidiNote(0, 60, 0.8f, -1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note1, 0));

        var note2 = new MidiNote(0, 62, 0.7f, -1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note2, 0));

        var note3 = new MidiNote(0, 64, 0.6f, -1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note3, 0));

        // ピッチ62にプレッシャーを適用する
        var pressure = new MidiPolyPressure(0, 62, 0.9f, MidiNote.GlobalNoteId);
        container.Reserve(new MidiEventReservation<MidiPolyPressure>(in pressure, 0));

        // ピッチ62のボイスのみにプレッシャーが適用されていることを確認
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.Pitch == 60)
            {
                Assert.Equal(0f, container.Voices[i].PolyPressure);
            }
            else if (container.Voices[i].Note.Pitch == 62)
            {
                Assert.Equal(0.9f, container.Voices[i].PolyPressure);
            }
            else if (container.Voices[i].Note.Pitch == 64)
            {
                Assert.Equal(0f, container.Voices[i].PolyPressure);
            }
        }
    }

    [Fact]
    public static void ProcessPolyPressure_異なるチャンネルには影響しない()
    {
        var container = new MidiVoiceContainer(2);

        // チャンネル0でノートオン
        var note1 = new MidiNote(0, 60, 0.8f, 100);
        container.Reserve(new MidiEventReservation<MidiNote>(in note1, 0));

        // チャンネル1でノートオン
        var note2 = new MidiNote(1, 60, 0.8f, 100);
        container.Reserve(new MidiEventReservation<MidiNote>(in note2, 0));

        // チャンネル0のピッチ60にプレッシャーを適用
        var pressure = new MidiPolyPressure(0, 60, 0.9f, 100);
        container.Reserve(new MidiEventReservation<MidiPolyPressure>(in pressure, 0));

        // チャンネル0のノートのみにプレッシャーが適用され、チャンネル1には影響しないことを確認
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.Channel == 0 && container.Voices[i].Note.IsOn)
            {
                Assert.Equal(0.9f, container.Voices[i].PolyPressure);
            }
            else if (container.Voices[i].Note.Channel == 1 && container.Voices[i].Note.IsOn)
            {
                Assert.Equal(0f, container.Voices[i].PolyPressure);
            }
        }
    }

    [Fact]
    public static void ProcessPolyPressure_複数の一致するボイスがある場合すべて更新する()
    {
        var container = new MidiVoiceContainer(4);

        // 同じピッチのノートを複数オンにする（NoteId = -1）
        var note1 = new MidiNote(0, 60, 0.8f, -1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note1, 0));

        var note2 = new MidiNote(0, 60, 0.7f, -1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note2, 0));

        var note3 = new MidiNote(0, 62, 0.6f, -1);
        container.Reserve(new MidiEventReservation<MidiNote>(in note3, 0));

        // ピッチ60にプレッシャーを適用
        var pressure = new MidiPolyPressure(0, 60, 0.9f, MidiNote.GlobalNoteId);
        container.Reserve(new MidiEventReservation<MidiPolyPressure>(in pressure, 0));

        // ピッチ60のすべてのボイスにプレッシャーが適用されていることを確認
        var pitch60Count = 0;
        var pitch60WithPressure = 0;
        for (var i = 0; i < container.Voices.Length; i++)
        {
            if (container.Voices[i].Note.IsOn && container.Voices[i].Note.Pitch == 60)
            {
                pitch60Count++;
                if (container.Voices[i].PolyPressure == 0.9f)
                {
                    pitch60WithPressure++;
                }
            }
        }

        Assert.Equal(2, pitch60Count);
        Assert.Equal(2, pitch60WithPressure);
    }
}
