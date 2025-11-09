using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class LowPassFilterTest
{
    [Fact]
    public static void TickAndRender_出力値が有限()
    {
        var filter = new LowPassFilter();
        var config = new FilterConfig(cutoff: 60, sampleRate: 44100);

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
        var filter = new LowPassFilter();
        var config = new FilterConfig(cutoff: 0, sampleRate: 44100);

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
        var filter = new LowPassFilter();
        var config = new FilterConfig(cutoff: 127, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_高周波を減衰させる()
    {
        var filter = new LowPassFilter();
        // 低いカットオフ周波数を設定
        var config = new FilterConfig(cutoff: 30, sampleRate: 44100);

        // 高周波ノイズのような入力を与える
        float previousOutput = 0f;
        for (var i = 0; i < 100; i++)
        {
            var input = (i % 2 == 0) ? 1.0f : -1.0f; // 矩形波（高周波成分を多く含む）
            var output = filter.TickAndRender(in config, input);

            if (i > 10) // 初期の過渡状態をスキップ
            {
                // 出力の変化が入力の変化より小さいことを確認（減衰している）
                var outputChange = MathF.Abs(output - previousOutput);
                Assert.True(outputChange < 1.8f, $"高周波が十分に減衰していない: {outputChange}");
            }
            previousOutput = output;
        }
    }
}
