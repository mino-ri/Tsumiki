using System;
using NPlug;

namespace Tsumiki;

public class TsumikiController : AudioController<TsumikiModel>
{
    private TsumikiViewModel? _viewModel;
    private TsumikiPluginView? _view;

    public static readonly Guid ClassId = new("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");

    public TsumikiController()
    {
        Model.Controller = this;
        SetMidiCCMapping(AudioMidiControllerNumber.PitchBend, Model.PitchBendParameter);
        SetMidiCCMapping(AudioMidiControllerNumber.ModWheel, Model.WheelParameter);
        SetMidiCCMapping(AudioMidiControllerNumber.AfterTouch, Model.AfterTouchParameter);
    }

    protected override IAudioPluginView? CreateView()
    {
        _viewModel ??= new TsumikiViewModel(Model, this);
        return _view ??= new TsumikiPluginView(_viewModel);
    }

    protected override void OnParameterValueChanged(AudioParameter parameter, bool parameterValueChangedFromHost)
    {
        base.OnParameterValueChanged(parameter, parameterValueChangedFromHost);
        _view?.OnParameterChanged(parameter.Id.Value);
    }
}
