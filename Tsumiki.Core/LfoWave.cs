using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
internal sealed class LfoWaveConfig
{
    private const float MaxSlope = 48000f;
    private readonly ILfoUnit _unit;
    private readonly IModulationSourceUnit[] _modulationSources;
    // ShapeX は上昇フェーズと下降フェーズのバランスを変える
    // ShapeY が正の値なら、サイン波 → 矩形波の変化
    // ShapeY が負の値なら、サイン波 → 三角波の変化
    private float _shapeX;
    private float _shapeY;
    public double Delta;
    public float Level;
    public float SinFactor;
    public float UpSlope;
    public float DownSlope;
    public bool IsActive;

    [InitTiming]
    public LfoWaveConfig(ILfoUnit unit, IModulationSourceUnit[] modulationSources, double sampleRate)
    {
        _unit = unit;
        _modulationSources = modulationSources;
        // 有効値は -1 ～ 1 なので、初期値として範囲外を設定することで計算を実行させる
        _shapeX = -2;
        _shapeY = -2;
        Recalculate(sampleRate);
    }

    [EventTiming]
    public void Recalculate(double sampleRate)
    {
        Delta = _unit.Speed / sampleRate;
        Level = _unit.Level;
        IsActive = Level > 0.0 && RecalculateIsActive();

        if (_shapeX != _unit.ShapeX || _shapeY != _unit.ShapeY)
        {
            _shapeX = _unit.ShapeX;
            _shapeY = _unit.ShapeY;
            SinFactor = _shapeY > 0f ? 1f : 1f + _shapeY;
            var flatness = Math.Min(1f - _shapeY, 1f);
            UpSlope = Math.Min(MaxSlope, 4f / flatness / (1f + _shapeX));
            DownSlope = -Math.Min(MaxSlope, 4f / flatness / (1f - _shapeX));
        }
    }

    [EventTiming]
    private bool RecalculateIsActive()
    {
        foreach (var unit in _modulationSources)
        {
            if (unit.Lfo != 0.0) return true;
        }

        return false;
    }
}

[AudioTiming]
[method: InitTiming]
internal struct LfoWave(PitchModulation pitchModulation, MultiplyModulation levelModulation, LfoWaveConfig config)
{
    private readonly PitchModulation _pitchModulation = pitchModulation;
    private readonly MultiplyModulation _levelModulation = levelModulation;
    private readonly LfoWaveConfig _config = config;
    double _phase;

    [EventTiming]
    public void Reset()
    {
        _phase = 0f;
    }

    [AudioTiming]
    public float TickAndRender()
    {
        if (!_config.IsActive) return 0f;

        var up = Math.Min(1f, Math.Abs((float)(_phase - Math.Round(_phase)) * _config.UpSlope));
        var down = (float)(_phase - (int)_phase - 0.5) * _config.DownSlope;
        var output = MathT.TriToSin2(Math.Clamp(down, -up, up), _config.SinFactor);

        _phase += _config.Delta * _pitchModulation.Render();
        _phase -= (int)_phase;

        return output * _config.Level * (float)_levelModulation.Render();
    }
}
