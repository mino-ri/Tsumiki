using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class ResonantLowPassFilterConfigTest
{
    /// <summary>テスト用のフィルターユニットのモック</summary>
    private class TestFilterUnit : IFilterUnit
    {
        public float Mix { get; set; }
        public float MorphFactor { get; set; }
        public int Cutoff { get; set; }
        public float Resonance { get; set; }
    }

    [Fact]
    public static void ResonantLowPassFilterConfig_通常のパラメータで正常に動作()
    {
        var unit = new TestFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit, pitch: 60, sampleRate: 44100);

        // Alpha と Damping が有効な範囲内であることを確認
        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.InRange(config.Damping, 0f, 1f);
    }

    [Fact]
    public static void ResonantLowPassFilterConfig_ピッチ0でも発散しない()
    {
        var unit = new TestFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit, pitch: 0, sampleRate: 44100);

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
        var unit = new TestFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit, pitch: 127, sampleRate: 44100);

        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.False(float.IsNaN(config.Alpha), "Alpha が NaN になっている");
        Assert.False(float.IsInfinity(config.Alpha), "Alpha が無限大になっている");

        Assert.InRange(config.Damping, 0f, 1f);
        Assert.False(float.IsNaN(config.Damping), "Damping が NaN になっている");
        Assert.False(float.IsInfinity(config.Damping), "Damping が無限大になっている");
    }

    [Fact]
    public static void ResonantLowPassFilterConfig_レゾナンス最大でも発散しない()
    {
        var unit = new TestFilterUnit { Cutoff = 0, Resonance = 0.98f };
        var config = new ResonantLowPassFilterConfig(unit, pitch: 60, sampleRate: 44100);

        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.InRange(config.Damping, 0f, 1f);
        Assert.False(float.IsNaN(config.Damping));
        Assert.False(float.IsInfinity(config.Damping));
    }
}
