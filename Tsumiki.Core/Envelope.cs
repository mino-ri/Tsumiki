using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
internal sealed class EnvelopeConfig
{
    private readonly IEnvelopeUnit _unit;
    private double _sampleRate;
    private int _attack;
    private int _decay;
    private int _release;

    public double AttackDelta;
    public double DecayRate;
    public double SustainLevel;
    public double ReleaseRate;

    [InitTiming]
    public EnvelopeConfig(IEnvelopeUnit unit, double sampleRate)
    {
        _unit = unit;
        Recalculate(sampleRate);
    }

    [EventTiming]
    public void Recalculate(double sampleRate)
    {
        var isSampleRateChanged = _sampleRate != sampleRate;
        _sampleRate = sampleRate;
        SustainLevel = _unit.Sustain;

        if (isSampleRateChanged || _attack != _unit.Attack)
        {
            _attack = _unit.Attack;
            AttackDelta = MathT.GetEnvAttackDelta(_attack, _sampleRate);
        }

        if (isSampleRateChanged || _decay != _unit.Decay)
        {
            _decay = _unit.Decay;
            DecayRate = MathT.GetEnvelopeRate(_decay, _sampleRate);
        }

        if (isSampleRateChanged || _release != _unit.Release)
        {
            _release = _unit.Release;
            ReleaseRate = MathT.GetEnvelopeRate(_release, _sampleRate);
        }
    }
}

/// <summary>一般的なADSRエンベロープを表します。</summary>
[AudioTiming]
internal struct Envelope(EnvelopeConfig config)
{
    const double MaxThreshold = 1.0 - MathT.ExpThreshold;
    private readonly EnvelopeConfig _config = config;
    double _level;
    bool _decaying;

    [EventTiming]
    public void Restart()
    {
        _decaying = false;
    }

    [EventTiming]
    public void Reset()
    {
        _level = 0;
        _decaying = false;
    }

    [AudioTiming]
    public float TickAndRender(bool noteOn)
    {
        if (!noteOn)
        {
            // 減衰途中からでも次のノート・オンに対応するために、予め _decaying を false にしておく
            _decaying = false;

            if (_level <= 0.0)
            {
                return 0f;
            }

            _level -= _level * _config.ReleaseRate;
            if (_level <= MathT.ExpThreshold)
            {
                _level = 0.0;
                return 0f;
            }

            return (float)(_level - MathT.ExpThreshold);
        }
        else if (!_decaying)
        {
            _level += _config.AttackDelta;
            if (_level >= MaxThreshold)
            {
                _level = 1.0;
                _decaying = true;
                return 1f;
            }

            return (float)(_level + MathT.ExpThreshold);
        }
        else if (_level <= _config.SustainLevel)
        {
            return (float)_level;
        }
        else
        {
            _level += (_config.SustainLevel - _level) * _config.DecayRate;
            if (_level <= _config.SustainLevel + MathT.ExpThreshold)
            {
                _level = _config.SustainLevel;
                return (float)_level;
            }

            return (float)(_level - MathT.ExpThreshold);
        }
    }
}
