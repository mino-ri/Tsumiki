using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class HighPassFilterTest
{
    [Fact]
    public static void TickAndRender_出力値が有限()
    {
        var filter = new HighPassFilter();
        var config = new FilterConfig(cutoffPitchNumber: 60, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_ピッチ0でも発散しない()
    {
        var filter = new HighPassFilter();
        var config = new FilterConfig(cutoffPitchNumber: 0, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_ピッチ127でも発散しない()
    {
        var filter = new HighPassFilter();
        var config = new FilterConfig(cutoffPitchNumber: 127, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_DC成分を減衰させる()
    {
        var filter = new HighPassFilter();
        var config = new FilterConfig(cutoffPitchNumber: 60, sampleRate: 44100);

        // DC成分（一定値）を入力
        float lastOutput = 0f;
        for (var i = 0; i < 200; i++)
        {
            lastOutput = filter.TickAndRender(in config, input: 1.0f);
        }

        // 十分に時間が経過すると、DC成分は除去されて出力は0に近づくはず
        Assert.InRange(lastOutput, -0.1f, 0.1f);
    }
}
