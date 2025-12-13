using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class ResonantLowPassFilterConfigTest
{
    [Fact]
    public static void ResonantLowPassFilterConfig_通常のパラメータで正常に動作()
    {
        var unit = new MockFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit, sampleRate: 44100);
        config.RecalculatePitch(60);

        // Alpha と Damping が有効な範囲内であることを確認
        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.InRange(config.Damping, 0f, 1f);
    }

    [Fact]
    public static void ResonantLowPassFilterConfig_ピッチ0でも発散しない()
    {
        var unit = new MockFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit, sampleRate: 44100);
        config.RecalculatePitch(0);

        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.False(float.IsNaN(config.Alpha), "Alpha が NaN になっている");
        Assert.False(float.IsInfinity(config.Alpha), "Alpha が無限大になっている");

        Assert.InRange(config.Damping, 0f, 1f);
        Assert.False(float.IsNaN(config.Damping), "Damping が NaN になっている");
        Assert.False(float.IsInfinity(config.Damping), "Damping が無限大になっている");
    }

    [Fact]
    public static void ResonantLowPassFilterConfig_ピッチ127でも発散しない()
    {
        var unit = new MockFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit, sampleRate: 44100);
        config.RecalculatePitch(127);

        Assert.False(float.IsNaN(config.Alpha), "Alpha が NaN になっている");
        Assert.False(float.IsInfinity(config.Alpha), "Alpha が無限大になっている");

        Assert.InRange(config.Damping, 0f, 1f);
        Assert.False(float.IsNaN(config.Damping), "Damping が NaN になっている");
        Assert.False(float.IsInfinity(config.Damping), "Damping が無限大になっている");
    }

    [Fact]
    public static void ResonantLowPassFilterConfig_レゾナンス最大でも発散しない()
    {
        var unit = new MockFilterUnit { Cutoff = 0, Resonance = 0.98f };
        var config = new ResonantLowPassFilterConfig(unit, sampleRate: 44100);
        config.RecalculatePitch(60);

        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.InRange(config.Damping, 0f, 1f);
        Assert.False(float.IsNaN(config.Damping));
        Assert.False(float.IsInfinity(config.Damping));
    }
}
