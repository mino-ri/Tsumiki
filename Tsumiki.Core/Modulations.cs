using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
internal class ModulationConfig
{
    private readonly ITsumikiModel _model;
    private readonly IModulationSourceUnit[] _destinations;
    public readonly LfoWaveConfig LfoWave;
    public readonly EnvelopeConfig Envelope;
    public readonly ModulationDestinationConfig LfoSpeedDest;
    public readonly ModulationDestinationConfig LfoLevelDest;
    public readonly ModulationDestinationConfig[] PitchDest;
    public readonly ModulationDestinationConfig[] PanDest;
    public readonly ModulationDestinationConfig[] LevelDest;
    public readonly ModulationDestinationConfig[] FmLevelDest;
    public readonly ModulationDestinationConfig FilterCutoffDest;
    public readonly ModulationDestinationConfig FilterResonanceDest;
    public readonly ModulationDestinationConfig FilterMixDest;
    public float LfoLevel;
    public float EnvelopeLevel;
    public bool EnvelopeActive;
    public float Wheel;

    [InitTiming]
    public ModulationConfig(ITsumikiModel model, double sampleRate)
    {
        _model = model;
        var unit = model.Modulation;
        _destinations =
        [
            unit.LfoSpeed,
            unit.LfoLevel,
            unit.APitch,
            unit.APan,
            unit.A1Level,
            unit.A2Level,
            unit.BPitch,
            unit.BPan,
            unit.B1Level,
            unit.B2Level,
            unit.FilterCutoff,
            unit.FilterResonance,
            unit.FilterMix,
        ];
        LfoSpeedDest = new(model, unit.LfoSpeed);
        LfoLevelDest = new(model, unit.LfoLevel);
        PitchDest = [new(model, unit.APitch), new(model, unit.BPitch)];
        PanDest = [new(model, unit.APan), new(model, unit.BPan)];
        LevelDest = [new(model, unit.A1Level), new(model, unit.B1Level)];
        FmLevelDest = [new(model, unit.A2Level), new(model, unit.B2Level)];
        FilterCutoffDest = new(model, unit.FilterCutoff);
        FilterResonanceDest = new(model, unit.FilterResonance);
        FilterMixDest = new(model, unit.FilterMix);

        LfoWave = new(unit.Lfo, _destinations, sampleRate);
        LfoLevel = unit.Lfo.Level;
        Envelope = new(unit.Envelope, sampleRate);
        EnvelopeLevel = unit.Envelope.Level;
        EnvelopeActive = EnvelopeLevel > 0.0 && RecalculateEnvelopeActive();
    }

    [EventTiming]
    public void Recalculate(double sampleRate)
    {
        LfoWave.Recalculate(sampleRate);
        Envelope.Recalculate(sampleRate);
        LfoSpeedDest.Recalculate();
        LfoLevelDest.Recalculate();
        PitchDest[0].Recalculate();
        PitchDest[1].Recalculate();
        PanDest[0].Recalculate();
        PanDest[1].Recalculate();
        LevelDest[0].Recalculate();
        LevelDest[1].Recalculate();
        FmLevelDest[0].Recalculate();
        FmLevelDest[1].Recalculate();
        FilterCutoffDest.Recalculate();
        FilterResonanceDest.Recalculate();
        FilterMixDest.Recalculate();
        EnvelopeLevel = _model.Modulation.Envelope.Level;
        EnvelopeActive = EnvelopeLevel > 0.0 && RecalculateEnvelopeActive();
        Wheel = _model.Wheel;
    }

    [EventTiming]
    private bool RecalculateEnvelopeActive()
    {
        foreach (var unit in _destinations)
        {
            if (unit.Env != 0.0) return true;
        }

        return false;
    }
}

[EventTiming]
[method: InitTiming]
internal class ModulationDestinationConfig(ITsumikiModel model, IModulationSourceUnit unit)
{
    private readonly ITsumikiModel _model = model;
    private readonly IModulationSourceUnit _unit = unit;
    public double Lfo = unit.Lfo;
    public double Envelope = unit.Env;
    public double Wheel = unit.Wheel;
    public double Velocity = unit.Velocity;
    public double Pressure = unit.Pressure;
    public double MultipliedWheel = model.Wheel * unit.Wheel;
    public bool IsActive = unit.Lfo != 0.0 || unit.Env != 0.0 || unit.Wheel != 0.0 || unit.Velocity != 0.0 || unit.Pressure != 0.0;

    [EventTiming]
    public void Recalculate()
    {
        Lfo = _unit.Lfo;
        Envelope = _unit.Env;
        Wheel = _unit.Wheel;
        Velocity = _unit.Velocity;
        Pressure = _unit.Pressure;
        MultipliedWheel = _model.Wheel * _unit.Wheel;
        IsActive = _unit.Lfo != 0.0 || _unit.Env != 0.0 || _unit.Wheel != 0.0 || _unit.Velocity != 0.0 || _unit.Pressure != 0.0;
    }
}

[AudioTiming]
internal class Modulation
{
    public readonly ModulationConfig Config;
    private LfoWave _lfoWave;
    private Envelope _envelope;
    public float Lfo;
    public float Envelope;
    public float Velocity;
    public float Pressure;

    [InitTiming]
    public Modulation(ModulationConfig config)
    {
        Config = config;
        _lfoWave = new(
            new PitchModulation(config.LfoSpeedDest, this),
            new MultiplyModulation(config.LfoLevelDest, this),
            config.LfoWave);
        _envelope = new(config.Envelope);
    }

    [EventTiming]
    public void StartNote()
    {
        _lfoWave.Reset();
        _envelope.Reset();
    }

    [EventTiming]
    public void RestartNote()
    {
        _envelope.Restart();
    }

    [AudioTiming]
    public void Tick(bool noteOn, float velocity, float pressure)
    {
        Lfo = _lfoWave.TickAndRender();
        Envelope = Config.EnvelopeActive ? _envelope.TickAndRender(noteOn) * Config.EnvelopeLevel : 0f;
        Velocity = velocity;
        Pressure = pressure;
    }
}

[InitTiming]
[method: InitTiming]
internal readonly struct AddModulation(ModulationDestinationConfig config, Modulation modulation)
{
    private readonly Modulation _modulation = modulation;
    private readonly ModulationDestinationConfig _config = config;

    [AudioTiming]
    public double Render()
    {
        return _config.IsActive ?
            _modulation.Lfo * _config.Lfo +
            _modulation.Envelope * _config.Envelope +
            _config.MultipliedWheel +
            _modulation.Pressure * _config.Pressure +
            _modulation.Velocity * _config.Velocity
            : 0.0;
    }
}

[InitTiming]
[method: InitTiming]
internal readonly struct MultiplyModulation(ModulationDestinationConfig config, Modulation modulation)
{
    private readonly ModulationConfig _modulationConfig = modulation.Config;
    private readonly Modulation _modulation = modulation;
    private readonly ModulationDestinationConfig _config = config;

    [AudioTiming]
    public double Render()
    {
        return _config.IsActive ?
            (Math.Min(1.0, 1.0 - _config.Lfo * _modulationConfig.LfoLevel) + (_modulation.Lfo + 1.0) * 0.5 * _config.Lfo) *
            (Math.Min(1.0, 1.0 - _config.Envelope * _modulationConfig.EnvelopeLevel) + _modulation.Envelope * _config.Envelope) *
            (Math.Min(1.0, 1.0 - _config.Wheel) + _config.MultipliedWheel) *
            (Math.Min(1.0, 1.0 - _config.Pressure) + _modulation.Pressure * _config.Pressure) *
            (Math.Min(1.0, 1.0 - _config.Velocity) + _modulation.Velocity * _config.Velocity)
            : 1.0;
    }
}

[InitTiming]
[method: InitTiming]
internal readonly struct PitchModulation(ModulationDestinationConfig config, Modulation modulation)
{
    private readonly Modulation _modulation = modulation;
    private readonly ModulationDestinationConfig _config = config;

    [AudioTiming]
    public double Render()
    {
        return _config.IsActive ?
            (1.0 + _modulation.Lfo * _config.Lfo) *
            (1.0 + _modulation.Envelope * _config.Envelope) *
            (1.0 + _config.MultipliedWheel) *
            (1.0 + _modulation.Pressure * _config.Pressure) *
            (1.0 + _modulation.Velocity * _config.Velocity)
            : 1.0;
    }
}
