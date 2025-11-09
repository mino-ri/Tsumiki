using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
[method: EventTiming]
internal readonly struct EnvelopeConfig(int attack, int decay, float sustain, int release, double sampleRate)
{
    public readonly double AttackDelta = MathT.GetEnvAttackDelta(attack, sampleRate);
    public readonly double DecayRate = MathT.GetEnvelopeRate(decay, sampleRate);
    public readonly double SustainLevel = sustain;
    public readonly double ReleaseRate = MathT.GetEnvelopeRate(release, sampleRate);

    public EnvelopeConfig(ICarrierUnit unit, double sampleRate)
        : this(unit.Attack, unit.Decay, unit.Sustain, unit.Release, sampleRate) { }

    public EnvelopeConfig(IModulatorUnit unit, double sampleRate)
        : this(unit.Attack, unit.Decay, unit.Sustain, unit.Release, sampleRate) { }

    public EnvelopeConfig(IEnvelopeUnit unit, double sampleRate)
        : this(unit.Attack, unit.Decay, unit.Sustain, unit.Release, sampleRate) { }
}

/// <summary>一般的なADSRエンベロープを表します。</summary>
[AudioTiming]
internal struct Envelope
{
    const double MaxThreashold = 1.0 - MathT.ExpThreshold;
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
    public float TickAndRender(in EnvelopeConfig config, bool noteOn)
    {
        if (!noteOn)
        {
            // 減衰途中からでも次のノート・オンに対応するために、予め _decaying を false にしておく
            _decaying = false;

            if (_level <= 0.0)
            {
                return 0f;
            }

            _level -= _level * config.ReleaseRate;
            if (_level <= MathT.ExpThreshold)
            {
                _level = 0.0;
                return 0f;
            }

            return (float)(_level - MathT.ExpThreshold);
        }
        else if (!_decaying)
        {
            _level += config.AttackDelta;
            if (_level >= MaxThreashold)
            {
                _level = 1.0;
                _decaying = true;
                return 1f;
            }

            return (float)(_level + MathT.ExpThreshold);
        }
        else if (_level <= config.SustainLevel)
        {
            return (float)_level;
        }
        else
        {
            _level += (config.SustainLevel - _level) * config.DecayRate;
            if (_level <= config.SustainLevel + MathT.ExpThreshold)
            {
                _level = config.SustainLevel;
                return (float)_level;
            }

            return (float)(_level - MathT.ExpThreshold);
        }
    }
}
