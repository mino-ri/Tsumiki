using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
internal readonly struct FilterConfig
{
    public readonly float Alpha;

    [EventTiming]
    public FilterConfig(double cutoff, double pitch, double sampleRate)
    {
        double rc = 1.0 / (2.0 * Math.PI * Math.Min(sampleRate / 2.0, MathT.PitchToFreq(cutoff + pitch)));
        double dt = 1.0 / sampleRate;
        Alpha = (float)(dt / (rc + dt));
    }
}

[AudioTiming]
internal struct LowPassFilter
{
    private float _lastOutput;

    [EventTiming]
    public void Reset()
    {
        _lastOutput = 0f;
    }

    [AudioTiming]
    public float TickAndRender(in FilterConfig config, float input)
    {
        _lastOutput += config.Alpha * (input - _lastOutput);
        return _lastOutput;
    }
}

[AudioTiming]
internal struct HighPassFilter
{
    private float _lastOutput;
    private float _lastInput;

    [EventTiming]
    public void Reset()
    {
        _lastOutput = 0f;
        _lastInput = 0f;
    }

    [AudioTiming]
    public float TickAndRender(in FilterConfig config, float input)
    {
        _lastOutput = config.Alpha * (_lastOutput + input - _lastInput);
        _lastInput = input;
        return _lastOutput;
    }
}

[EventTiming]
[method: EventTiming]
internal readonly struct ResonantLowPassFilterConfig(IFilterUnit unit, double pitch, double sampleRate)
{
    public readonly float Alpha = MathF.Min(1f, 2.0f * MathT.Sin((float)(MathT.PitchToFreq(unit.Cutoff + pitch) / sampleRate / 2.0)));
    public readonly float Damping = 1f - unit.Resonance;
}

[AudioTiming]
internal struct ResonantLowPassFilter
{
    private float _low;
    private float _band;

    [EventTiming]
    public void Reset()
    {
        _low = 0f;
        _band = 0f;
    }

    [AudioTiming]
    public float TickAndRender(in ResonantLowPassFilterConfig config, float input)
    {
        _low += config.Alpha * _band;
        var high = input - _low - config.Damping * _band;
        _band += config.Alpha * high;

        return _low;
    }
}
