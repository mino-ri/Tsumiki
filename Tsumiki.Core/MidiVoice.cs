using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
[method: EventTiming]
public readonly struct MidiNote(short channel, short pitch, float velocity, int noteId)
{
    public const float OffVelocity = -1f;
    public const int GlobalNoteId = -1;
    public static readonly MidiNote Off = new(0, 0, OffVelocity, GlobalNoteId);

    public readonly short Channel = channel;
    public readonly short Pitch = pitch;
    public readonly float Velocity = velocity;
    public readonly int NoteId = noteId;

    public readonly bool IsOn => Velocity >= 0f;

    [EventTiming]
    public bool IsSame(in MidiNote other)
    {
        if (Channel != other.Channel) return false;

        // NoteId のどちらか一方でも GlobalNoteId の場合はピッチで比較
        if (NoteId == GlobalNoteId || other.NoteId == GlobalNoteId)
            return Pitch == other.Pitch;

        // それ以外は NoteId で比較
        return NoteId == other.NoteId;
    }

    [EventTiming]
    public bool IsSame(in MidiPolyPressure other)
    {
        if (Channel != other.Channel) return false;

        // NoteId のどちらか一方でも GlobalNoteId の場合はピッチで比較
        if (NoteId == GlobalNoteId || other.NoteId == GlobalNoteId)
            return Pitch == other.Pitch;

        // それ以外は NoteId で比較
        return NoteId == other.NoteId;
    }
}

[EventTiming]
[method: EventTiming]
public readonly struct MidiPolyPressure(short channel, short pitch, float pressure, int noteId)
{
    public readonly short Channel = channel;
    public readonly short Pitch = pitch;
    public readonly float Pressure = pressure;
    public readonly int NoteId = noteId;
}

[AudioTiming]
[method: EventTiming]
public struct MidiEventReservation<T>(in T @event, int sampleOffset)
    where T : struct
{
    public readonly T Event = @event;
    /// <summary>発音を開始するまでに必要なサンプル数</summary>
    public int SampleOffset = sampleOffset;
}

[AudioTiming]
[method: EventTiming]
internal struct MidiVoice(in MidiNote note)
{
    public readonly MidiNote Note = note;
    public int Length = 0;
    public float PolyPressure = 0f;
}

[InitTiming]
internal readonly struct MidiVoiceContainer
{
    public readonly MidiEventReservation<MidiNote>[] Reservations;
    public readonly MidiEventReservation<MidiPolyPressure>[] PressureReservations;
    public readonly MidiVoice[] Voices;

    [InitTiming]
    public MidiVoiceContainer(int voiceCount)
    {
        Reservations = GC.AllocateArray<MidiEventReservation<MidiNote>>(voiceCount, true);
        PressureReservations = GC.AllocateArray<MidiEventReservation<MidiPolyPressure>>(voiceCount, true);
        Voices = GC.AllocateArray<MidiVoice>(voiceCount, true);

        for (var i = 0; i < voiceCount; i++)
        {
            Reservations[i] = new(in MidiNote.Off, -1);
            PressureReservations[i] = new(default, -1);
            Voices[i] = new(in MidiNote.Off);
        }
    }

    [EventTiming]
    private static void Reserve<T>(MidiEventReservation<T>[] storage, in MidiEventReservation<T> reservation)
        where T : struct
    {

        var oldestIndex = 0;
        for (var i = 0; i < storage.Length; i++)
        {
            if (storage[i].SampleOffset <= 0)
            {
                storage[i] = reservation;
                return;
            }
            if (storage[i].SampleOffset > storage[oldestIndex].SampleOffset)
            {
                oldestIndex = i;
            }
        }

        // 予約枠が空いていない場合、最も古いものを上書きする
        storage[oldestIndex] = reservation;
    }

    /// <summary>ノート・オンまたはノート・オフを予約します。</summary>
    [EventTiming]
    public readonly void Reserve(in MidiEventReservation<MidiNote> reservation)
    {
        if (reservation.SampleOffset <= 0)
        {
            ProcessNote(in reservation.Event);
        }
        else
        {
            Reserve(Reservations, in reservation);
        }
    }

    /// <summary>アフタータッチ(ポリプレッシャー)を予約します。</summary>
    [EventTiming]
    public readonly void Reserve(in MidiEventReservation<MidiPolyPressure> reservation)
    {
        if (reservation.SampleOffset <= 0)
        {
            ProcessPolyPressure(in reservation.Event);
        }
        else
        {
            Reserve(PressureReservations, in reservation);
        }
    }

    /// <summary>時刻を1サンプル分進めます。</summary>
    [AudioTiming]
    public readonly void Tick()
    {
        // 発音予約の実行
        for (var i = 0; i < Reservations.Length; i++)
        {
            ref var reservation = ref Reservations[i];
            if (reservation.SampleOffset < 0)
                continue;

            reservation.SampleOffset--;
            if (reservation.SampleOffset == -1)
            {
                // EVENT CALL
                ProcessNote(in reservation.Event);
            }
        }

        // アフタータッチ予約の実行
        for (var i = 0; i < PressureReservations.Length; i++)
        {
            ref var reservation = ref PressureReservations[i];
            if (reservation.SampleOffset < 0)
                continue;

            reservation.SampleOffset--;
            if (reservation.SampleOffset == -1)
            {
                // EVENT CALL
                ProcessPolyPressure(in reservation.Event);
            }
        }

        // 発音済みボイスの時間進行
        for (var i = 0; i < Voices.Length; i++)
        {
            Voices[i].Length++;
        }
    }

    /// <summary>ノート・オンまたはノート・オフを実行します。</summary>
    [EventTiming]
    private readonly void ProcessNote(in MidiNote note)
    {
        if (note.IsOn)
        {
            NoteOn(in note);
        }
        else
        {
            NoteOff(in note);
        }
    }

    [EventTiming]
    private readonly void NoteOn(in MidiNote note)
    {
        var oldestOnIndex = 0;
        var targetIndex = -1;
        for (var i = 0; i < Voices.Length; i++)
        {
            ref var voice = ref Voices[i];
            if (voice.Note.IsOn)
            {
                if (Voices[oldestOnIndex].Length < voice.Length)
                {
                    oldestOnIndex = i;
                }
            }
            else if (targetIndex == -1 || Voices[targetIndex].Length < voice.Length)
            {
                targetIndex = i;
            }
        }

        if (targetIndex == -1)
        {
            targetIndex = oldestOnIndex;
        }

        Voices[targetIndex] = new MidiVoice(in note);
    }

    [EventTiming]
    private readonly void NoteOff(in MidiNote note)
    {
        for (var i = 0; i < Voices.Length; i++)
        {
            if (Voices[i].Note.IsOn && Voices[i].Note.IsSame(in note))
            {
                Voices[i] = new MidiVoice(in MidiNote.Off);
            }
        }
    }

    [EventTiming]
    private readonly void ProcessPolyPressure(in MidiPolyPressure polyPressure)
    {
        for (var i = 0; i < Voices.Length; i++)
        {
            if (Voices[i].Note.IsOn && Voices[i].Note.IsSame(in polyPressure))
            {
                Voices[i].PolyPressure = polyPressure.Pressure;
            }
        }
    }
}
