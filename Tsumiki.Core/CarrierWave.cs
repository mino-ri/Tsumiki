using Tsumiki.Metadata;

namespace Tsumiki.Core;

[EventTiming]
internal readonly struct CarrierWaveConfig
{
    // ShapeX は上昇フェーズと下降フェーズのバランスを変える
    // ShapeY が正の値なら、三角波 → 矩形波の変化
    // ShapeY が負の値なら、三角波 → サイン波の変化
    public readonly double Pitch;
    public readonly float Phase;
    public readonly bool Sync;
    public readonly float Level;
    public readonly float TriFactor;
    public readonly float SinFactor;
    public readonly float UpSlope;
    public readonly float DownSlope;
    public readonly float UpEnd;
    public readonly float DownStart;

    [EventTiming]
    public CarrierWaveConfig(ICarrierUnit unit)
    {
        Pitch = unit.Pitch;
        Phase = unit.Phase;
        Sync = unit.Sync;
        Level = unit.Level;
        TriFactor = unit.ShapeY > 0f ? 2f : (1f + unit.ShapeY) * 2f;
        SinFactor = unit.ShapeY > 0f ? 0f : -unit.ShapeY;
        var flatness = unit.ShapeY > 0f ? 1f - unit.ShapeY : 1f;
        UpSlope = 1f / flatness / (0.5f + unit.ShapeX * 0.5f);
        DownSlope = 1f / flatness / (0.5f - unit.ShapeX * 0.5f);
        UpEnd = (0.25f + unit.ShapeX * 0.25f) * flatness;
        DownStart = 0.5f - (0.25f - unit.ShapeX * 0.25f) * flatness;
    }
}

[AudioTiming]
internal struct CarrierWave
{
    double _phase;

    [EventTiming]
    public void Reset(in CarrierWaveConfig config)
    {
        _phase = config.Phase;
    }

    [AudioTiming]
    public float TickAndRender(in CarrierWaveConfig config, double delta, double syncPhase, float fm)
    {
        if (config.Sync && syncPhase >= 0)
        {
            _phase = delta * config.Pitch + config.Phase;
        }

        var dActualPhase = _phase + fm;
        // 扱いやすさのため、 -0.5～0.5 に正規化する
        var actualPhase = (float)(dActualPhase - (int)dActualPhase - 0.5f);
        var absPhase = Math.Abs((float)actualPhase);

        var output =
            absPhase < config.UpEnd ? actualPhase * config.UpSlope
            : absPhase > config.DownStart ? (MathF.Sign(actualPhase) * 0.5f - actualPhase) * config.DownSlope
            : actualPhase >= 0f ? 0.5f : -0.5f;

        if (!float.IsFinite(output))
        {
            output = float.IsNaN(output)
                ? 0f : float.IsPositive(output)
                ? 0.5f : -0.5f;
        }

        output = config.SinFactor > 0f
            ? output * config.TriFactor + MathT.TriToSin(output) * config.SinFactor
            : output * config.TriFactor;

        _phase += delta * config.Pitch;
        _phase -= (int)_phase;

        return output * config.Level;
    }
}
