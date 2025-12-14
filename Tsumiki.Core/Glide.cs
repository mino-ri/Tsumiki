using System.Runtime.CompilerServices;
using Tsumiki.Metadata;

namespace Tsumiki.Core;

[AudioTiming]
[method: InitTiming]
internal struct Glide(GliderConfig glideConfig)
{
    private const double threshold = 1.0 / 128.0;
    private readonly GliderConfig _config = glideConfig;
    private double _target;
    private double _current;

    [AudioTiming]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTarget(double value)
    {
        _target = value;
    }

    [AudioTiming]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset(double value)
    {
        _target = value;
        _current = value;
    }

    [AudioTiming]
    public double TickAndRender()
    {
        var diff = _target - _current;
        if (Math.Abs(diff) <= threshold)
        {
            _current = _target;
            return _current;
        }

        _current += diff * _config.GlideRate;
        return _current;
    }
}
