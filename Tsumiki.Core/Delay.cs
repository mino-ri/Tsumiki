using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
[method: EventTiming]
internal readonly struct DelayConfig(IDelayUnit unit, double sampleRate)
{
    public readonly FilterConfig LowCutConfig = new(unit.LowCut, sampleRate);
    public readonly FilterConfig HighCutConfig = new(unit.HighCut, sampleRate);
    public readonly int DelaySampleCount = (int)(unit.Delay * sampleRate / 1000);
    public readonly float Feedback = unit.Feedback;
    public readonly bool Cross = unit.Cross;
}

[AudioTiming]
[method: InitTiming]
internal struct DelayChannel(double sampleRate)
{
    const float FeedbackThreshold = 1f / 8192f;

    private float[] _buffer = GC.AllocateArray<float>((int)(sampleRate / 2.0), true);
    private int _currentIndex;
    private HighPassFilter _lowCut;
    private LowPassFilter _highCut;

    public float Feedback { get; private set; }

    [AudioTiming]
    public float TickAndRender(in DelayConfig config, float input)
    {
        _currentIndex = (_currentIndex + 1) % config.DelaySampleCount;
        var output = _buffer[_currentIndex];
        _buffer[_currentIndex] = input;

        output = _lowCut.TickAndRender(in config.LowCutConfig, output);
        output = _highCut.TickAndRender(in config.HighCutConfig, output);
        Feedback = output * config.Feedback;
        if (Math.Abs(Feedback) <= FeedbackThreshold)
            Feedback = 0f;

        return output;
    }
}

[AudioTiming]
[method: InitTiming]
internal struct Delay(double sampleRate)
{
    private DelayChannel _left = new(sampleRate);
    private DelayChannel _right = new(sampleRate);

    [AudioTiming]
    public (float left, float right) TickAndRender(in DelayConfig config, float left, float right)
    {
        var leftInput = left + _left.Feedback;
        var rightInput = right + _right.Feedback;
        if (config.Cross)
        {
            (leftInput, rightInput) = (rightInput, leftInput);
        }

        return (_left.TickAndRender(in config, leftInput), _right.TickAndRender(in config, rightInput));
    }
}
