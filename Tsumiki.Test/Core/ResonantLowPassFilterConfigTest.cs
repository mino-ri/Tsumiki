using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class ResonantLowPassFilterConfigTest
{
    [Fact]
    public static void ResonantLowPassFilterConfig_通常のパラメータで正常に動作()
    {
        var unit = new FilterFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit);

        Assert.Equal(Math.PI * 2.0, config.Alpha, 0.0001f);
        Assert.InRange(config.Damping, 0f, 1f);
        Assert.False(float.IsNaN(config.Damping));
        Assert.False(float.IsInfinity(config.Damping));
    }

    [Fact]
    public static void ResonantLowPassFilterConfig_ピッチ最低でも発散しない()
    {
        var unit = new FilterFilterUnit { Cutoff = -60, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit);

        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.False(double.IsNaN(config.Alpha), "Alpha が NaN になっている");
        Assert.False(double.IsInfinity(config.Alpha), "Alpha が無限大になっている");

        Assert.InRange(config.Damping, 0f, 1f);
        Assert.False(float.IsNaN(config.Damping), "Damping が NaN になっている");
        Assert.False(float.IsInfinity(config.Damping), "Damping が無限大になっている");
    }

    [Fact]
    public static void ResonantLowPassFilterConfig_ピッチ最高でも発散しない()
    {
        var unit = new FilterFilterUnit { Cutoff = 60, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit);

        Assert.False(double.IsNaN(config.Alpha), "Alpha が NaN になっている");
        Assert.False(double.IsInfinity(config.Alpha), "Alpha が無限大になっている");

        Assert.InRange(config.Damping, 0f, 1f);
        Assert.False(float.IsNaN(config.Damping), "Damping が NaN になっている");
        Assert.False(float.IsInfinity(config.Damping), "Damping が無限大になっている");
    }

    [Fact]
    public static void ResonantLowPassFilterConfig_レゾナンス最大でも発散しない()
    {
        var unit = new FilterFilterUnit { Cutoff = 0, Resonance = 0.98f };
        var config = new ResonantLowPassFilterConfig(unit);

        Assert.Equal(Math.PI * 2.0, config.Alpha, 0.0001f);
        Assert.InRange(config.Damping, 0f, 1f);
        Assert.False(float.IsNaN(config.Damping));
        Assert.False(float.IsInfinity(config.Damping));
    }
}
