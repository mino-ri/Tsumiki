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

    private static readonly AudioProgramListBuilder<TsumikiModel> DefaultProgramListBuilder = CreateAudioProgramListBuilder();

    private static AudioProgramListBuilder<TsumikiModel> CreateAudioProgramListBuilder()
    {
        return new AudioProgramListBuilder<TsumikiModel>("Bank")
        {
            model =>
            {
                InitModel(model);
                return new("Default");
            },
            model =>
            {
                InitModel(model);
                return new("Sine");
            },
        };
    }

    private static void InitModel(TsumikiModel model)
    {
        for (var i = 0; i < 300; i++)
        {
            if (model.TryGetParameterById(new AudioParameterId(i), out var parameter))
            {
                parameter.NormalizedValue = parameter.DefaultNormalizedValue;
            }
        }
    }
}
