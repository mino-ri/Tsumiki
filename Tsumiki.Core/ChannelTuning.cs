namespace Tsumiki.Core;

internal struct TuningValue(int n, int d, int pn, int pd)
{
    private int _n = n;
    private int _d = d;
    private int _pn = pn;
    private int _pd = pd;
    public double Value = Math.Log2((double)n / d) * 12.0 * pn / pd;

    public bool Updated(int n, int d, int pn, int pd)
    {
        var shouldUpdate = _n != n || _d != d || _pn != pn || _pd != pd;
        if (shouldUpdate)
        {
            _n = n;
            _d = d;
            _pn = pn;
            _pd = pd;
            Value = Math.Log2((double)n / d) * 12.0 * pn / pd;
        }

        return shouldUpdate;
    }
}

internal class ChannelTuningConfig(ITuningUnit tuningUnit, IChannelTuningUnit unit)
{
    // public double[] PitchValues = GC.AllocateArray<double>(128, true);
    private int _root = tuningUnit.Root;
    private int _keyPeriod = tuningUnit.KeyPeriod;
    private int _offset = unit.Offset;
    private TuningValue _ratio = new(unit.RatioN, unit.RatioD, unit.RatioPn, unit.RatioPd);
    private TuningValue _generator = new(unit.GeneratorN, unit.GeneratorD, unit.GeneratorPn, unit.GeneratorPd);
    private TuningValue _period = new(unit.PeriodN, unit.PeriodD, unit.PeriodPn, unit.PeriodPd);

    public void Recalculate()
    {
        var newOffset = unit.Offset;
        var newRoot = tuningUnit.Root;
        var newKeyPeriod = tuningUnit.KeyPeriod;
        if (_offset == newOffset &&
            _root == newRoot &&
            _keyPeriod == newKeyPeriod &&
            !_ratio.Updated(unit.RatioN, unit.RatioD, unit.RatioPn, unit.RatioPd) &&
            !_generator.Updated(unit.GeneratorN, unit.GeneratorD, unit.GeneratorPn, unit.GeneratorPd) &&
            !_period.Updated(unit.PeriodN, unit.PeriodD, unit.PeriodPn, unit.PeriodPd))
        {
            return;
        }

        _offset = unit.Offset;
        _root = tuningUnit.Root;
        _keyPeriod = tuningUnit.KeyPeriod;
        var basePitch = _root + _ratio.Value;

        // KeyCount は 1～127 の範囲なので常に stack alloc が可能
        Span<double> periodPitches = stackalloc double[_keyPeriod];
        for (var i = 0; i < _keyPeriod; i++)
        {
            var factor = i - _offset;
            var targetPitch = _generator.Value * factor % _period.Value;
            if (factor < 0 && targetPitch != 0.0)
            {
                targetPitch += _period.Value;
            }

            targetPitch += basePitch;
            periodPitches[i] = targetPitch;
        }

        periodPitches.Sort();

        for (var i = 0; i < 128; i++)
        {
            var relativeKey = i - _root;
            var (periodCount, index) = Math.DivRem(relativeKey, _keyPeriod);
            if (relativeKey < 0 && index != 0)
            {
                periodCount--;
                index += _keyPeriod;
            }

            unit.SetPitch(i, Math.Clamp(_period.Value * periodCount + periodPitches[index], 0.0, 128.0));
        }
    }
}
