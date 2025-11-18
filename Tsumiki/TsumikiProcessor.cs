using System;
using NPlug;
using Tsumiki.Core;

namespace Tsumiki;

public class TsumikiProcessor : AudioProcessor<TsumikiModel>
{
    private double _sampleRate;
    private readonly Processor _processor;

    public static readonly Guid ClassId = new("f6e5d4c3-b2a1-4d5e-9f8e-7d6c5b4a3210");

    public override Guid ControllerClassId => TsumikiController.ClassId;

    public TsumikiProcessor() : base(AudioSampleSizeSupport.Float32)
    {
        _sampleRate = ProcessSetupData.SampleRate;
        _processor = new Processor(Model, _sampleRate);
    }

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
        if (_sampleRate != ProcessSetupData.SampleRate)
        {
            _sampleRate = ProcessSetupData.SampleRate;
            _processor.ChangeSampleRate(_sampleRate);
        }
        else
        {
            _processor.Recalculate();
        }
    }

    protected override void ProcessMain(in AudioProcessData data)
    {
        if (_sampleRate != ProcessSetupData.SampleRate)
        {
            _sampleRate = ProcessSetupData.SampleRate;
            _processor.ChangeSampleRate(_sampleRate);
        }

        var leftChannel = data.Output[0].GetChannelSpanAsFloat32(ProcessSetupData, data, 0);
        var rightChannel = data.Output[0].GetChannelSpanAsFloat32(ProcessSetupData, data, 1);
        var sampleCount = data.SampleCount;

        if (!_processor.IsActive)
        {
            leftChannel.Clear();
            rightChannel.Clear();
            return;
        }

        _processor.ProcessMain(sampleCount, leftChannel, rightChannel);
    }
}
