using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
[method: InitTiming]
internal sealed class ModulatorWaveConfig(IModulatorUnit unit)
{
    private readonly IModulatorUnit _unit = unit;
    public double Pitch = unit.Pitch;
    public float Phase = unit.Phase;
    public float Feedback = unit.Feedback;
    public bool Sync = unit.Sync;
    public float Level = unit.Level;

    [EventTiming]
    public void Recalculate()
    {
        Pitch = _unit.Pitch;
        Phase = _unit.Phase;
        Feedback = _unit.Feedback;
        Sync = _unit.Sync;
        Level = _unit.Level;
    }
}

[AudioTiming]
[method: InitTiming]
internal struct ModulatorWave(ModulatorWaveConfig config)
{
    private readonly ModulatorWaveConfig _config = config;
    double _phase;
    double _output;

    [EventTiming]
    public void Reset()
    {
        _phase = _config.Phase;
        _output = 0;
    }

    [AudioTiming]
    public float TickAndRender(double delta, double syncPhase)
    {
        if (_config.Sync && syncPhase >= 0)
        {
            _phase = delta * _config.Pitch + _config.Phase;
            _phase -= (int)_phase;
        }

        var actualPhase = _phase + _config.Feedback * _output;
        actualPhase -= Math.Round(actualPhase);
        _output = MathT.Sin((float)actualPhase);
        _phase += delta * _config.Pitch;
        _phase -= (int)_phase;

        return (float)_output * _config.Level;
    }
}
