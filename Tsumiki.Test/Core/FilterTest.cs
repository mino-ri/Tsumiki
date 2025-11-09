using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class FilterTest
{
    /// <summary>テスト用のフィルターユニットのモック</summary>
    private class TestFilterUnit : IFilterUnit
    {
        public float Mix { get; set; }
        public float MorphFactor { get; set; }
        public int Cutoff { get; set; }
        public float Resonance { get; set; }
    }

    #region FilterConfig Tests

    [Fact]
    public static void FilterConfig_通常のパラメータで正常に動作()
    {
        var config = new FilterConfig(cutoff: 0, pitch: 60, sampleRate: 44100);

        // Alpha は 0 と 1 の間の値であるべき
        Assert.InRange(config.Alpha, 0f, 1f);
    }

    [Fact]
    public static void FilterConfig_ピッチ0でも発散しない()
    {
        var config = new FilterConfig(cutoff: 0, pitch: 0, sampleRate: 44100);

        // Alpha が有効な範囲内であることを確認
        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.False(float.IsNaN(config.Alpha), "Alpha が NaN になっている");
        Assert.False(float.IsInfinity(config.Alpha), "Alpha が無限大になっている");
    }

    [Fact]
    public static void FilterConfig_ピッチ127でも発散しない()
    {
        var config = new FilterConfig(cutoff: 0, pitch: 127, sampleRate: 44100);

        // Alpha が有効な範囲内であることを確認
        Assert.InRange(config.Alpha, 0f, 1f);
        Assert.False(float.IsNaN(config.Alpha), "Alpha が NaN になっている");
        Assert.False(float.IsInfinity(config.Alpha), "Alpha が無限大になっている");
    }

    [Fact]
    public static void FilterConfig_極端なカットオフでも発散しない()
    {
        var config1 = new FilterConfig(cutoff: -64, pitch: 60, sampleRate: 44100);
        var config2 = new FilterConfig(cutoff: 64, pitch: 60, sampleRate: 44100);

        Assert.InRange(config1.Alpha, 0f, 1f);
        Assert.False(float.IsNaN(config1.Alpha));
        Assert.False(float.IsInfinity(config1.Alpha));

        Assert.InRange(config2.Alpha, 0f, 1f);
        Assert.False(float.IsNaN(config2.Alpha));
        Assert.False(float.IsInfinity(config2.Alpha));
    }

    #endregion

    #region LowPassFilter Tests

    [Fact]
    public static void LowPassFilter_TickAndRender_出力値が有限()
    {
        var filter = new LowPassFilter();
        var config = new FilterConfig(cutoff: 0, pitch: 60, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void LowPassFilter_TickAndRender_ピッチ0でも発散しない()
    {
        var filter = new LowPassFilter();
        var config = new FilterConfig(cutoff: 0, pitch: 0, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void LowPassFilter_TickAndRender_ピッチ127でも発散しない()
    {
        var filter = new LowPassFilter();
        var config = new FilterConfig(cutoff: 0, pitch: 127, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void LowPassFilter_TickAndRender_高周波を減衰させる()
    {
        var filter = new LowPassFilter();
        // 低いカットオフ周波数を設定
        var config = new FilterConfig(cutoff: -30, pitch: 60, sampleRate: 44100);

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

    #endregion

    #region HighPassFilter Tests

    [Fact]
    public static void HighPassFilter_TickAndRender_出力値が有限()
    {
        var filter = new HighPassFilter();
        var config = new FilterConfig(cutoff: 0, pitch: 60, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void HighPassFilter_TickAndRender_ピッチ0でも発散しない()
    {
        var filter = new HighPassFilter();
        var config = new FilterConfig(cutoff: 0, pitch: 0, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void HighPassFilter_TickAndRender_ピッチ127でも発散しない()
    {
        var filter = new HighPassFilter();
        var config = new FilterConfig(cutoff: 0, pitch: 127, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void HighPassFilter_TickAndRender_DC成分を減衰させる()
    {
        var filter = new HighPassFilter();
        var config = new FilterConfig(cutoff: 0, pitch: 60, sampleRate: 44100);

        // DC成分（一定値）を入力
        float lastOutput = 0f;
        for (var i = 0; i < 200; i++)
        {
            lastOutput = filter.TickAndRender(in config, input: 1.0f);
        }

        // 十分に時間が経過すると、DC成分は除去されて出力は0に近づくはず
        Assert.InRange(lastOutput, -0.1f, 0.1f);
    }

    #endregion

    #region ResonantLowPassFilterConfig Tests

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

    #endregion

    #region ResonantLowPassFilter Tests

    [Fact]
    public static void ResonantLowPassFilter_TickAndRender_出力値が有限()
    {
        var filter = new ResonantLowPassFilter();
        var unit = new TestFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit, pitch: 60, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void ResonantLowPassFilter_TickAndRender_ピッチ0でも発散しない()
    {
        var filter = new ResonantLowPassFilter();
        var unit = new TestFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit, pitch: 0, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void ResonantLowPassFilter_TickAndRender_ピッチ127でも発散しない()
    {
        var filter = new ResonantLowPassFilter();
        var unit = new TestFilterUnit { Cutoff = 0, Resonance = 0.5f };
        var config = new ResonantLowPassFilterConfig(unit, pitch: 127, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = filter.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void ResonantLowPassFilter_TickAndRender_高レゾナンスでも発散しない()
    {
        var filter = new ResonantLowPassFilter();
        var unit = new TestFilterUnit { Cutoff = 0, Resonance = 0.98f };
        var config = new ResonantLowPassFilterConfig(unit, pitch: 60, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            // より変動の大きい入力を与える
            var input = MathF.Sin(i * 0.1f);
            var output = filter.TickAndRender(in config, input);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
            Assert.InRange(output, -10f, 10f); // レゾナンスで増幅されるが有限であることを確認
        }
    }

    [Fact]
    public static void ResonantLowPassFilter_TickAndRender_レゾナンスで特性が変化する()
    {
        var filterNoRes = new ResonantLowPassFilter();
        var filterWithRes = new ResonantLowPassFilter();

        var unitNoRes = new TestFilterUnit { Cutoff = 0, Resonance = 0.02f };
        var unitWithRes = new TestFilterUnit { Cutoff = 0, Resonance = 0.9f };

        var configNoRes = new ResonantLowPassFilterConfig(unitNoRes, pitch: 60, sampleRate: 44100);
        var configWithRes = new ResonantLowPassFilterConfig(unitWithRes, pitch: 60, sampleRate: 44100);

        var resultsNoRes = new float[200];
        var resultsWithRes = new float[200];

        for (var i = 0; i < 200; i++)
        {
            // インパルス入力
            var input = (i == 0) ? 1.0f : 0.0f;
            resultsNoRes[i] = filterNoRes.TickAndRender(in configNoRes, input);
            resultsWithRes[i] = filterWithRes.TickAndRender(in configWithRes, input);
        }

        // レゾナンスが高い方が応答が持続する（遅くゼロに収束する）はず
        // 後半のサンプルで差が出ることを確認
        var sumNoRes = 0f;
        var sumWithRes = 0f;
        for (var i = 50; i < 200; i++)
        {
            sumNoRes += MathF.Abs(resultsNoRes[i]);
            sumWithRes += MathF.Abs(resultsWithRes[i]);
        }

        Assert.True(sumWithRes > sumNoRes, "レゾナンスが高い方が応答が持続するはず");
    }

    #endregion
}
