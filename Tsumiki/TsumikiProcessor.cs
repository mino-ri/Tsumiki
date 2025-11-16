using System;
using NPlug;
using Tsumiki.Core;

namespace Tsumiki;

public class TsumikiProcessor()
    : AudioProcessor<TsumikiModel>(AudioSampleSizeSupport.Float32)
{
    private readonly Core.Processor _processor = new();

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
        _processor.OnActive(isActive, Model, ProcessSetupData.SampleRate);
    }

    protected override void ProcessEvent(in AudioEvent audioEvent)
    {
        switch (audioEvent.Kind)
        {
            case AudioEventKind.NoteOn:
                ref readonly var noteOn = ref audioEvent.Value.NoteOn;
                _processor.ReserveNote(new(new MidiNote(noteOn.Channel, noteOn.Pitch, noteOn.Velocity, noteOn.NoteId), audioEvent.SampleOffset));
                break;

            case AudioEventKind.NoteOff:
                ref readonly var noteOff = ref audioEvent.Value.NoteOff;
                _processor.ReserveNote(new(new MidiNote(noteOff.Channel, noteOff.Pitch, MidiNote.OffVelocity, noteOff.NoteId), audioEvent.SampleOffset));
                break;

            case AudioEventKind.LegacyMIDICCOut:
                ref readonly var cc = ref audioEvent.Value.MidiCCOut;
                break;

            case AudioEventKind.PolyPressure:
                ref readonly var polyPressure = ref audioEvent.Value.PolyPressure;
                break;
        }
    }

    protected override void ProcessRecalculate(in AudioProcessData data)
    {
        _processor.Recalculate(Model, ProcessSetupData.SampleRate);
    }

    protected override void ProcessMain(in AudioProcessData data)
    {
        var leftChannel = data.Output[0].GetChannelSpanAsFloat32(ProcessSetupData, data, 0);
        var rightChannel = data.Output[0].GetChannelSpanAsFloat32(ProcessSetupData, data, 1);
        var sampleCount = data.SampleCount;

        if (!_processor.IsActive)
        {
            leftChannel.Clear();
            rightChannel.Clear();
            return;
        }

        _processor.ProcessMain(Model, ProcessSetupData.SampleRate, sampleCount, leftChannel, rightChannel);
    }
}
