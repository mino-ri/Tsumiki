using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
internal sealed class OperatorWaveConfig
{
    private const float MaxSlope = 48000f;
    private readonly IOperatorUnit _unit;
    // ShapeX は上昇フェーズと下降フェーズのバランスを変える
    // ShapeY が正の値なら、サイン波 → 矩形波の変化
    // ShapeY が負の値なら、サイン波 → 三角波の変化
    private float _shapeX;
    private float _shapeY;
    public double Pitch;
    public bool Sync;
    public float Level;
    public float SinFactor;
    public float UpSlope;
    public float DownSlope;

    [InitTiming]
    public OperatorWaveConfig(IOperatorUnit unit)
    {
        _unit = unit;
        // 有効値は -1 ～ 1 なので、初期値として範囲外を設定することで計算を実行させる
        _shapeX = -2;
        _shapeY = -2;
        Recalculate();
    }

    [EventTiming]
    public void Recalculate()
    {
        Pitch = _unit.Pitch;
        Sync = _unit.Sync;
        Level = _unit.Level;

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
}

[AudioTiming]
[method: InitTiming]
internal struct OperatorWave(OperatorWaveConfig config)
{
    private readonly OperatorWaveConfig _config = config;
    double _phase;

    [EventTiming]
    public void Reset()
    {
        _phase = 0f;
    }

    [AudioTiming]
    public float TickAndRender(double delta, double syncPhase, float fm)
    {
        if (_config.Level == 0f) return 0f;

        if (_config.Sync && syncPhase >= 0)
        {
            _phase = delta * _config.Pitch;
        }

        var dActualPhase = _phase + fm + 1.0;
        var up = Math.Min(1f, Math.Abs((float)(dActualPhase - (int)(dActualPhase + 0.5)) * _config.UpSlope));
        var down = (float)(dActualPhase - (int)dActualPhase - 0.5) * _config.DownSlope;
        var output = MathT.TriToSin2(Math.Clamp(down, -up, up), _config.SinFactor);

        _phase += delta * _config.Pitch;
        _phase -= (int)_phase;

        return output * _config.Level;
    }
}
