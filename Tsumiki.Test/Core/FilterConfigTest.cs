using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class FilterConfigTest
{
    [Fact]
    public static void FilterConfig_通常のパラメータで正常に動作()
    {
        var config = new FilterConfig(cutoffPitchNumber: 60, sampleRate: 44100);

        // Alpha は 0 と 1 の間の値であるべき
        Assert.InRange(config.Alpha, 0f, 1f);
    }

    [Fact]
    public static void FilterConfig_ピッチ0でも発散しない()
    {
        var config = new FilterConfig(cutoffPitchNumber: 0, sampleRate: 44100);

        // Alpha が有効な範囲内であることを確認
        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.False(float.IsNaN(config.Alpha), "Alpha が NaN になっている");
        Assert.False(float.IsInfinity(config.Alpha), "Alpha が無限大になっている");
    }

    [Fact]
    public static void FilterConfig_ピッチ127でも発散しない()
    {
        var config = new FilterConfig(cutoffPitchNumber: 127, sampleRate: 44100);

        // Alpha が有効な範囲内であることを確認
        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.False(float.IsNaN(config.Alpha), "Alpha が NaN になっている");
        Assert.False(float.IsInfinity(config.Alpha), "Alpha が無限大になっている");
    }
}
