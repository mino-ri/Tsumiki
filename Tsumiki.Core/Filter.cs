using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
internal readonly struct FilterConfig
{
    public readonly float Alpha;

    [EventTiming]
    public FilterConfig(double cutoffPitchNumber, double sampleRate)
    {
        double rc = 1.0 / (2.0 * Math.PI * Math.Min(sampleRate / 2.0, MathT.PitchToFreq(cutoffPitchNumber)));
        double dt = 1.0 / sampleRate;
        Alpha = MathF.Min(1f, (float)(dt / (rc + dt)));
    }
}

[EventTiming]
internal readonly struct FilterConfigD
{
    public readonly double Alpha;

    [EventTiming]
    public FilterConfigD(double cutoffPitchNumber, double sampleRate)
    {
        double rc = 1.0 / (2.0 * Math.PI * Math.Min(sampleRate / 2.0, MathT.PitchToFreq(cutoffPitchNumber)));
        double dt = 1.0 / sampleRate;
        Alpha = Math.Min(1.0, dt / (rc + dt));
    }
}

[AudioTiming]
internal struct LowPassFilter
{
    private float _lastOutput;

    [EventTiming]
    public void Reset(float resetValue = 0f)
    {
        _lastOutput = resetValue;
    }

    [AudioTiming]
    public float TickAndRender(in FilterConfig config, float input)
    {
        _lastOutput += config.Alpha * (input - _lastOutput);
        return _lastOutput;
    }
}

[AudioTiming]
internal struct LowPassFilterD
{
    private double _lastOutput;

    [EventTiming]
    public void Reset(double resetValue = 0f)
    {
        _lastOutput = resetValue;
    }

    [AudioTiming]
    public double TickAndRender(in FilterConfigD config, double input)
    {
        _lastOutput += config.Alpha * (input - _lastOutput);
        return _lastOutput;
    }
}

[AudioTiming]
internal struct HighPassFilter
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
        return input - _lastOutput;
    }
}

[AudioTiming]
[method: EventTiming]
internal struct ResonantLowPassFilterConfig(IFilterUnit unit, double pitch, double sampleRate)
{
    public readonly float Cutoff = unit.Cutoff;
    public readonly float Damping = 1f - unit.Resonance;
    public float Alpha = MathF.Min(1f, 2.0f * MathT.Sin((float)(MathT.PitchToFreq(unit.Cutoff + pitch) / sampleRate / 2.0)));

    [AudioTiming]
    public void RecalculatePitch(double pitch, double sampleRate)
    {
        Alpha = MathF.Min(1f, 2.0f * MathT.Sin((float)(MathT.PitchToFreq(Cutoff + pitch) / sampleRate / 2.0)));
    }
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
