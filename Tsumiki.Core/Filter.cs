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
    private double _frequencyFactor = 2.0 * MathF.PI / sampleRate;
    private double _pitch = 128.0;
    public float _cutoff = unit.Cutoff;
    public float Damping = 1f - unit.Resonance;
    public float Alpha = (float)(MathT.PitchToFreq(unit.Cutoff) * 2.0 * MathF.PI / sampleRate);

    [EventTiming]
    public void Recalculate(double sampleRate)
    {
        _sampleRate = sampleRate;
        _cutoff = unit.Cutoff;
        _frequencyFactor = 2.0 * MathF.PI / _sampleRate;
        Damping = 1f - unit.Resonance;
        Alpha = (float)(MathT.PitchToFreq(_cutoff + _pitch) * _frequencyFactor);
    }

    [AudioTiming]
    public void RecalculatePitch(double pitch)
    {
        if (_pitch == pitch)
            return;
        _pitch = pitch;
        Alpha = (float)(MathT.PitchToFreq(_cutoff + _pitch) * _frequencyFactor);
    }
}

[AudioTiming]
[method: InitTiming]
internal struct ResonantLowPassFilter(ResonantLowPassFilterConfig config, Modulation modulation)
{
    private readonly MultiplyModulation _cutoffModulation = new(modulation.Config.FilterCutoffDest, modulation);
    private readonly AddModulation _resonanceModulation = new(modulation.Config.FilterResonanceDest, modulation);
    private readonly ResonantLowPassFilterConfig _config = config;
    private float _leftLow;
    private float _leftBand;
    private float _rightLow;
    private float _rightBand;

    [EventTiming]
    public void Reset()
    {
        _leftLow = 0f;
        _leftBand = 0f;
        _rightLow = 0f;
        _rightBand = 0f;
    }

    [AudioTiming]
    public (float left, float right) TickAndRender(float left, float right)
    {
        var alpha = Math.Min(1f, _config.Alpha * (float)_cutoffModulation.Render());
        var damping = Math.Clamp(_config.Damping + (float)_resonanceModulation.Render(), 0.02f, 1f);

        _leftLow += alpha * _leftBand;
        var leftHigh = left - _leftLow - damping * _leftBand;
        _leftBand += alpha * leftHigh;

        _rightLow += alpha * _rightBand;
        var rightHigh = right - _rightLow - damping * _rightBand;
        _rightBand += alpha * rightHigh;

        return (_leftLow, _rightLow);
    }
}
