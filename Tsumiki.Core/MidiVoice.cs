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
}

[AudioTiming]
[method: EventTiming]
public struct MidiNoteReservation(in MidiNote note, int sampleOffset)
{
    public readonly MidiNote Note = note;
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
    public readonly MidiNoteReservation[] Reservations;
    public readonly MidiVoice[] Voices;

    [InitTiming]
    public MidiVoiceContainer(int voiceCount)
    {
        Reservations = GC.AllocateArray<MidiNoteReservation>(voiceCount, true);
        Voices = GC.AllocateArray<MidiVoice>(voiceCount, true);

        for (var i = 0; i < voiceCount; i++)
        {
            Reservations[i] = new MidiNoteReservation(in MidiNote.Off, -1);
            Voices[i] = new MidiVoice(in MidiNote.Off);
        }
    }

    /// <summary>ノート・オンまたはノート・オフを予約します。</summary>
    [EventTiming]
    public readonly void Reserve(in MidiNoteReservation reservation)
    {
        if (reservation.SampleOffset <= 0)
        {
            ProcessNote(in reservation.Note);
            return;
        }

        var oldestIndex = 0;
        for (var i = 0; i < Reservations.Length; i++)
        {
            if (Reservations[i].SampleOffset <= 0)
            {
                Reservations[i] = reservation;
                return;
            }
            if (Reservations[i].SampleOffset > Reservations[oldestIndex].SampleOffset)
            {
                oldestIndex = i;
            }
        }

        // 予約枠が空いていない場合、最も古いものを上書きする
        Reservations[oldestIndex] = reservation;
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
                ProcessNote(in reservation.Note);
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
}
