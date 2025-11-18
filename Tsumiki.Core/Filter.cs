using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
internal sealed class FilterConfig
{
    private double _cutoffPitchNumber;
    private double _sampleRate;
    public float Alpha;

    [InitTiming]
    public FilterConfig(double cutoffPitchNumber, double sampleRate)
    {
        Recalculate(cutoffPitchNumber, sampleRate);
    }

    [EventTiming]
    public void Recalculate(double cutoffPitchNumber, double sampleRate)
    {
        if (_cutoffPitchNumber == cutoffPitchNumber && _sampleRate == sampleRate)
            return;

        _cutoffPitchNumber = cutoffPitchNumber;
        _sampleRate = sampleRate;
        var rc = 1.0 / (2.0 * Math.PI * Math.Min(_sampleRate / 2.0, MathT.PitchToFreq(_cutoffPitchNumber)));
        var dt = 1.0 / _sampleRate;
        Alpha = MathF.Min(1f, (float)(dt / (rc + dt)));
    }
}

[EventTiming]
internal sealed class FilterConfigD
{
    private double _cutoffPitchNumber;
    private double _sampleRate;
    public double Alpha;

    [InitTiming]
    public FilterConfigD(double cutoffPitchNumber, double sampleRate)
    {
        Recalculate(cutoffPitchNumber, sampleRate);
    }

    [EventTiming]
    public void Recalculate(double cutoffPitchNumber, double sampleRate)
    {
        if (_cutoffPitchNumber == cutoffPitchNumber && _sampleRate == sampleRate)
            return;

        _cutoffPitchNumber = cutoffPitchNumber;
        _sampleRate = sampleRate;
        var rc = 1.0 / (2.0 * Math.PI * Math.Min(sampleRate / 2.0, MathT.PitchToFreq(cutoffPitchNumber)));
        var dt = 1.0 / sampleRate;
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
internal sealed class ResonantLowPassFilterConfig(IFilterUnit unit, double sampleRate)
{
    private double _sampleRate = sampleRate;
    private double _pitch = 128.0;
    public float _cutoff = unit.Cutoff;
    public float Damping = 1f - unit.Resonance;
    public float Alpha = MathF.Min(1f, 2.0f * MathT.Sin((float)(MathT.PitchToFreq(unit.Cutoff) / sampleRate / 2.0)));

    [EventTiming]
    public void Recalculate(double sampleRate)
    {
        _sampleRate = sampleRate;
        _cutoff = unit.Cutoff;
        Damping = 1f - unit.Resonance;
        Alpha = MathF.Min(1f, 2.0f * MathT.Sin((float)(MathT.PitchToFreq(_cutoff + _pitch) / _sampleRate / 2.0)));
    }

    [AudioTiming]
    public void RecalculatePitch(double pitch)
    {
        if (_pitch == pitch)
            return;
        _pitch = pitch;
        Alpha = MathF.Min(1f, 2.0f * MathT.Sin((float)(MathT.PitchToFreq(_cutoff + _pitch) / _sampleRate / 2.0)));
    }
}

[AudioTiming]
[method: InitTiming]
internal struct ResonantLowPassFilter(ResonantLowPassFilterConfig config)
{
    private readonly ResonantLowPassFilterConfig _config = config;
    private float _low;
    private float _band;

    [EventTiming]
    public void Reset()
    {
        _low = 0f;
        _band = 0f;
    }

    [AudioTiming]
    public float TickAndRender(float input)
    {
        _low += _config.Alpha * _band;
        var high = input - _low - _config.Damping * _band;
        _band += _config.Alpha * high;

        return _low;
    }
}
