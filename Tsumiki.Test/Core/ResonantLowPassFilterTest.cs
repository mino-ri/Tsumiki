using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class ResonantLowPassFilterTest
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
    public static void TickAndRender_出力値が有限()
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
    public static void TickAndRender_ピッチ0でも発散しない()
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
    public static void TickAndRender_ピッチ127でも発散しない()
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
    public static void TickAndRender_高レゾナンスでも発散しない()
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
    public static void TickAndRender_レゾナンスで特性が変化する()
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
}
