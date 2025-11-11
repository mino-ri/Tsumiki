using System;
using NPlug;

namespace Tsumiki;

public class TsumikiController : AudioController<TsumikiModel>
{
    public static readonly Guid ClassId = new("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");

    public TsumikiController()
    {
        SetMidiCCMapping(AudioMidiControllerNumber.PitchBend, Model.PitchBendParameter);
        SetMidiCCMapping(AudioMidiControllerNumber.ModWheel, Model.WheelParameter);
        SetMidiCCMapping(AudioMidiControllerNumber.AfterTouch, Model.AfterTouchParameter);
    }

    protected override IAudioPluginView? CreateView()
    {
        return new TsumikiPluginView(Model);
    }
}
