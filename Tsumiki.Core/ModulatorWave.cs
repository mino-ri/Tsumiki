using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
[method: EventTiming]
internal readonly struct ModulatorWaveConfig(IModulatorUnit unit)
{
    public readonly double Pitch = unit.Pitch;
    public readonly float Phase = unit.Phase;
    public readonly float Feedback = unit.Feedback;
    public readonly bool Sync = unit.Sync;
}

[AudioTiming]
internal struct ModulatorWave
{
    double _phase;
    double _output;

    [EventTiming]
    public void Reset(in ModulatorWaveConfig config)
    {
        _phase = config.Phase;
        _output = 0;
    }

    [AudioTiming]
    public float TickAndRender(in ModulatorWaveConfig config, double delta, double syncPhase)
    {
        if (config.Sync && syncPhase >= 0)
        {
            _phase += delta * config.Pitch + config.Phase;
        }

        var actualPhase = _phase + config.Feedback * _output;
        actualPhase -= Math.Floor(actualPhase);
        _output = MathT.Sin((float)actualPhase);
        _phase += delta * config.Pitch;

        if (_phase > 1.0)
        {
            _phase -= Math.Floor(_phase);
        }

        return (float)_output;
    }
}
