using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
internal sealed class CarrierWaveConfig
{
    private readonly ICarrierUnit _unit;
    private float _shapeX;
    private float _shapeY;
    // ShapeX は上昇フェーズと下降フェーズのバランスを変える
    // ShapeY が正の値なら、三角波 → 矩形波の変化
    // ShapeY が負の値なら、三角波 → サイン波の変化
    public double Pitch;
    public float Phase;
    public bool Sync;
    public float Level;
    public float TriFactor;
    public float SinFactor;
    public float UpSlope;
    public float DownSlope;
    public float UpEnd;
    public float DownStart;

    [InitTiming]
    public CarrierWaveConfig(ICarrierUnit unit)
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
        Phase = _unit.Phase;
        Sync = _unit.Sync;
        Level = _unit.Level;

        if (_shapeX != _unit.ShapeX || _shapeY != _unit.ShapeY)
        {
            _shapeX = _unit.ShapeX;
            _shapeY = _unit.ShapeY;
            TriFactor = _shapeY > 0f ? 2f : (1f + _shapeY) * 2f;
            SinFactor = _shapeY > 0f ? 0f : -_shapeY;
            var flatness = _shapeY > 0f ? 1f - _shapeY : 1f;
            UpSlope = 1f / flatness / (0.5f + _shapeX * 0.5f);
            DownSlope = 1f / flatness / (0.5f - _shapeX * 0.5f);
            UpEnd = (0.25f + _shapeX * 0.25f) * flatness;
            DownStart = 0.5f - (0.25f - _shapeX * 0.25f) * flatness;
        }
    }
}

[AudioTiming]
[method: InitTiming]
internal struct CarrierWave(CarrierWaveConfig config)
{
    private readonly CarrierWaveConfig _config = config;
    double _phase;

    [EventTiming]
    public void Reset()
    {
        _phase = _config.Phase;
    }

    [AudioTiming]
    public float TickAndRender(double delta, double syncPhase, float fm)
    {
        if (_config.Sync && syncPhase >= 0)
        {
            _phase = delta * _config.Pitch + _config.Phase;
        }

        var dActualPhase = _phase + fm;
        // 扱いやすさのため、 -0.5～0.5 に正規化する
        var actualPhase = (float)(dActualPhase - (int)dActualPhase - 0.5f);
        var absPhase = Math.Abs((float)actualPhase);

        var output =
            absPhase < _config.UpEnd ? actualPhase * _config.UpSlope
            : absPhase > _config.DownStart ? (MathF.Sign(actualPhase) * 0.5f - actualPhase) * _config.DownSlope
            : actualPhase >= 0f ? 0.5f : -0.5f;

        if (!float.IsFinite(output))
        {
            output = float.IsNaN(output)
                ? 0f : float.IsPositive(output)
                ? 0.5f : -0.5f;
        }

        output = _config.SinFactor > 0f
            ? output * _config.TriFactor + MathT.TriToSin(output) * _config.SinFactor
            : output * _config.TriFactor;

        _phase += delta * _config.Pitch;
        _phase -= (int)_phase;

        return output * _config.Level;
    }
}
