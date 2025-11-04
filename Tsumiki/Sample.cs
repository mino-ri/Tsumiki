using NPlug;

namespace Tsumiki;

public class SampleDelayModel : AudioProcessorModel
{
    public AudioParameter Delay { get; }

    public SampleDelayModel() : base("SampleDelay")
    {
        AddByPassParameter();
        Delay = AddParameter(new AudioParameter("Delay", units: "sec", defaultNormalizedValue: 1.0));
    }
}

public class SampleDelayController : AudioController<SampleDelayModel>
{
    public static readonly Guid ClassId = new("d57bb9cf-5b7c-4770-9967-e8aef98dfe1e");
}

public class SampleDelayProcessor()
    : AudioProcessor<SampleDelayModel>(AudioSampleSizeSupport.Float32)
{
    private float[] _bufferLeft = [];
    private float[] _bufferRight = [];
    private int _bufferPosition;

    public static readonly Guid ClassId = new("db017b1e-9eb8-475f-9cdb-4c6b07304020");

    public override Guid ControllerClassId => SampleDelayController.ClassId;

    protected override bool Initialize(AudioHostApplication host)
    {
        AddDefaultStereoAudioInput();
        AddDefaultStereoAudioOutput();
        AddEventInput("Event Input", 16);
        return true;
    }

    protected override void OnActivate(bool isActive)
    {
        if (isActive)
        {
            var delayInSamples = (int)(ProcessSetupData.SampleRate * sizeof(float) + 0.5);
            _bufferLeft = GC.AllocateArray<float>(delayInSamples, true);
            _bufferRight = GC.AllocateArray<float>(delayInSamples, true);
            _bufferPosition = 0;
        }
        else
        {
            _bufferLeft = [];
            _bufferRight = [];
            _bufferPosition = 0;
        }
    }

    protected override void ProcessEvent(in AudioEvent audioEvent)
    {
        base.ProcessEvent(audioEvent);
    }

    protected override void ProcessMain(in AudioProcessData data)
    {
        var delayInSamples = Math.Max(1, (int)(ProcessSetupData.SampleRate * Model.Delay.NormalizedValue));
        for (var channel = 0; channel < 2; channel++)
        {
            var inputChannel = data.Input[0].GetChannelSpanAsFloat32(ProcessSetupData, data, channel);
            var outputChannel = data.Output[0].GetChannelSpanAsFloat32(ProcessSetupData, data, channel);

            var sampleCount = data.SampleCount;
            var buffer = channel == 0 ? _bufferLeft : _bufferRight;
            var tempBufferPos = _bufferPosition;
            for (int sample = 0; sample < sampleCount; sample++)
            {
                var tempSample = inputChannel[sample];
                outputChannel[sample] = buffer[tempBufferPos];
                buffer[tempBufferPos] = tempSample;
                tempBufferPos++;
                if (tempBufferPos >= delayInSamples)
                {
                    tempBufferPos = 0;
                }
            }
        }

        _bufferPosition += data.SampleCount;
        while (_bufferPosition >= delayInSamples)
        {
            _bufferPosition -= delayInSamples;
        }
    }
}
