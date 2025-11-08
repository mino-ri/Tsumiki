using NPlug;
using Tsumiki.Core;
using Tsumiki.Metadata;

namespace Tsumiki;

[VstModel("Tsumiki", typeof(ITsumikiModel))]
public partial class TsumikiModel
{
    public AudioParameter PitchBendParameter => _pitchBend;

    public AudioParameter WheelParameter => _wheel;

    public AudioParameter AfterTouchParameter => _afterTouch;
}

public class TsumikiController : AudioController<TsumikiModel>
{
    public static readonly Guid ClassId = new("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");

    public TsumikiController()
    {
        SetMidiCCMapping(AudioMidiControllerNumber.PitchBend, Model.PitchBendParameter);
        SetMidiCCMapping(AudioMidiControllerNumber.ModWheel, Model.WheelParameter);
        SetMidiCCMapping(AudioMidiControllerNumber.AfterTouch, Model.AfterTouchParameter);
    }
}

public class TsumikiProcessor()
    : AudioProcessor<TsumikiModel>(AudioSampleSizeSupport.Float32)
{
    private readonly Core.TsumikiProcessor _processor = new();

    public static readonly Guid ClassId = new("f6e5d4c3-b2a1-4d5e-9f8e-7d6c5b4a3210");

    public override Guid ControllerClassId => TsumikiController.ClassId;

    protected override bool Initialize(AudioHostApplication host)
    {
        AddDefaultStereoAudioOutput();
        AddEventInput("MIDI Input", 16);
        return true;
    }

    protected override void OnActivate(bool isActive)
    {
        _processor.OnActive(isActive);
    }

    protected override void ProcessEvent(in AudioEvent audioEvent)
    {
        switch (audioEvent.Kind)
        {
            case AudioEventKind.NoteOn:
                ref readonly var noteOn = ref audioEvent.Value.NoteOn;
                _processor.OnNoteOn(noteOn.Pitch, noteOn.Velocity, noteOn.NoteId, ProcessSetupData.SampleRate, audioEvent.SampleOffset);
                break;

            case AudioEventKind.NoteOff:
                ref readonly var noteOff = ref audioEvent.Value.NoteOff;
                _processor.OnNoteOff(noteOff.Pitch, noteOff.NoteId, audioEvent.SampleOffset);
                break;

            case AudioEventKind.LegacyMIDICCOut:
                ref readonly var cc = ref audioEvent.Value.MidiCCOut;
                break;

            case AudioEventKind.PolyPressure:
                ref readonly var polyPressure = ref audioEvent.Value.PolyPressure;
                break;
        }
    }

    protected override void ProcessMain(in AudioProcessData data)
    {
        if (!_processor.IsActive)
            return;

        var leftChannel = data.Output[0].GetChannelSpanAsFloat32(ProcessSetupData, data, 0);
        var rightChannel = data.Output[0].GetChannelSpanAsFloat32(ProcessSetupData, data, 1);
        var sampleCount = data.SampleCount;
        _processor.ProcessMain(Model, sampleCount, leftChannel, rightChannel);
    }
}
