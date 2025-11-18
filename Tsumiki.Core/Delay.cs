using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
[method: EventTiming]
internal class DelayConfig(IDelayUnit unit, double sampleRate)
{
    private readonly IDelayUnit _unit = unit;
    public readonly FilterConfig LowCutConfig = new(unit.LowCut, sampleRate);
    public readonly FilterConfig HighCutConfig = new(unit.HighCut, sampleRate);
    public int DelaySampleCount = (int)(unit.Delay * sampleRate / 1000);
    public float Feedback = unit.Feedback;
    public bool Cross = unit.Cross;
    public float Mix = unit.Mix;

    [EventTiming]
    public void Recalculate(double sampleRate)
    {
        LowCutConfig.Recalculate(_unit.LowCut, sampleRate);
        HighCutConfig.Recalculate(_unit.HighCut, sampleRate);
        DelaySampleCount = (int)(_unit.Delay * sampleRate / 1000);
        Feedback = _unit.Feedback;
        Cross = _unit.Cross;
        Mix = _unit.Mix;
    }
}

[AudioTiming]
[method: InitTiming]
internal struct DelayChannel(DelayConfig config, double sampleRate)
{
    const float FeedbackThreshold = 1f / 8192f;

    private readonly DelayConfig _config = config;
    private readonly float[] _buffer = GC.AllocateArray<float>(Math.Max(24000, (int)(sampleRate / 2.0)), true);
    private int _currentIndex;
    private HighPassFilter _lowCut;
    private LowPassFilter _highCut;

    public float Feedback { get; private set; }

    [AudioTiming]
    public float TickAndRender(float input)
    {
        _currentIndex = (_currentIndex + 1) % _config.DelaySampleCount;
        var output = _buffer[_currentIndex];
        _buffer[_currentIndex] = input;

        output = _lowCut.TickAndRender(in _config.LowCutConfig, output);
        output = _highCut.TickAndRender(in _config.HighCutConfig, output);
        Feedback = output * _config.Feedback;
        if (Math.Abs(Feedback) <= FeedbackThreshold)
            Feedback = 0f;

        return output;
    }
}

[AudioTiming]
[method: InitTiming]
internal struct Delay(DelayConfig config, double sampleRate)
{
    private readonly DelayConfig _config = config;
    private DelayChannel _left = new(config, sampleRate);
    private DelayChannel _right = new(config, sampleRate);

    [AudioTiming]
    public (float left, float right) TickAndRender(float left, float right)
    {
        var leftInput = left + _left.Feedback;
        var rightInput = right + _right.Feedback;
        if (_config.Cross)
        {
            (leftInput, rightInput) = (rightInput, leftInput);
        }

        return (_left.TickAndRender(leftInput), _right.TickAndRender(rightInput));
    }
}
