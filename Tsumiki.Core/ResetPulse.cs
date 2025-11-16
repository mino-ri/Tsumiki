using Tsumiki.Metadata;

namespace Tsumiki.Core;

[AudioTiming]
internal struct ResetPulse
{
    private double _phase;

    [EventTiming]
    public void Reset()
    {
        _phase = 0.0; 
    }

    [AudioTiming]
    public double Tick(double delta)
    {
        _phase += delta;
        var resetFactor = (int)_phase;
        _phase -= resetFactor;

        return resetFactor >= 1 ? _phase : -1.0;
    }
}
