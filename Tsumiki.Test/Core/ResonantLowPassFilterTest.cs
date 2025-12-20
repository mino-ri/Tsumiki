using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class ResonantLowPassFilterTest
{
    [Fact]
    public static void TickAndRender_出力値が有限()
    {
        var model = new TsumikiModel();
        model.Filter.Cutoff = 0;
        model.Filter.Resonance = 0.5f;
        
        var modulationConfig = new ModulationConfig(model, sampleRate: 44100);
        var modulation = new Modulation(modulationConfig);
        var config = new ResonantLowPassFilterConfig(model.Filter);
        var filter = new ResonantLowPassFilter(config, modulation);
        var delta = MathT.PitchToDelta(60, 44100);

        for (var i = 0; i < 1000; i++)
        {
            var (left, right) = filter.TickAndRender(1.0f, 1.0f, delta);
            Assert.False(float.IsNaN(left), $"サンプル {i} で NaN が発生 (Left)");
            Assert.False(float.IsInfinity(left), $"サンプル {i} で無限大が発生 (Left)");
            Assert.False(float.IsNaN(right), $"サンプル {i} で NaN が発生 (Right)");
            Assert.False(float.IsInfinity(right), $"サンプル {i} で無限大が発生 (Right)");
        }
    }

    [Fact]
    public static void TickAndRender_ピッチ0でも発散しない()
    {
        var model = new TsumikiModel();
        model.Filter.Cutoff = 0;
        model.Filter.Resonance = 0.5f;

        var modulationConfig = new ModulationConfig(model, 44100);
        var modulation = new Modulation(modulationConfig);
        var config = new ResonantLowPassFilterConfig(model.Filter);
        var filter = new ResonantLowPassFilter(config, modulation);
        var delta = MathT.PitchToDelta(0, 44100);

        for (var i = 0; i < 1000; i++)
        {
            var (left, right) = filter.TickAndRender(1.0f, 1.0f, delta);
            Assert.False(float.IsNaN(left), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(left), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_ピッチ127でも発散しない()
    {
        var model = new TsumikiModel();
        model.Filter.Cutoff = 0;
        model.Filter.Resonance = 0.5f;

        var modulationConfig = new ModulationConfig(model, 44100);
        var modulation = new Modulation(modulationConfig);
        var config = new ResonantLowPassFilterConfig(model.Filter);
        var filter = new ResonantLowPassFilter(config, modulation);
        var delta = MathT.PitchToDelta(127, 44100);

        for (var i = 0; i < 1000; i++)
        {
            var (left, right) = filter.TickAndRender(1.0f, 1.0f, delta);
            Assert.False(float.IsNaN(left), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(left), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_高レゾナンスでも発散しない()
    {
        var model = new TsumikiModel();
        model.Filter.Cutoff = 0;
        model.Filter.Resonance = 0.98f;

        var modulationConfig = new ModulationConfig(model, 44100);
        var modulation = new Modulation(modulationConfig);
        var config = new ResonantLowPassFilterConfig(model.Filter);
        var filter = new ResonantLowPassFilter(config, modulation);
        var delta = MathT.PitchToDelta(60, 44100);

        for (var i = 0; i < 1000; i++)
        {
            // より変動の大きい入力を与える
            var input = MathF.Sin(i * 0.1f);
            var (left, right) = filter.TickAndRender(input, input, delta);
            Assert.False(float.IsNaN(left), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(left), $"サンプル {i} で無限大が発生");
            Assert.InRange(left, -10f, 10f); // レゾナンスで増幅されるが有限であることを確認
        }
    }

    [Fact]
    public static void TickAndRender_レゾナンスで特性が変化する()
    {
        var modelNoRes = new TsumikiModel();
        modelNoRes.Filter.Cutoff = 0;
        modelNoRes.Filter.Resonance = 0.02f;

        var modelWithRes = new TsumikiModel();
        modelWithRes.Filter.Cutoff = 0;
        modelWithRes.Filter.Resonance = 0.9f;

        var modConfigNoRes = new ModulationConfig(modelNoRes, 44100);
        var modNoRes = new Modulation(modConfigNoRes);
        var configNoRes = new ResonantLowPassFilterConfig(modelNoRes.Filter);
        var filterNoRes = new ResonantLowPassFilter(configNoRes, modNoRes);

        var modConfigWithRes = new ModulationConfig(modelWithRes, 44100);
        var modWithRes = new Modulation(modConfigWithRes);
        var configWithRes = new ResonantLowPassFilterConfig(modelWithRes.Filter);
        var filterWithRes = new ResonantLowPassFilter(configWithRes, modWithRes);

        var resultsNoRes = new float[200];
        var resultsWithRes = new float[200];
        var delta = MathT.PitchToDelta(60, 44100);

        for (var i = 0; i < 200; i++)
        {
            // インパルス入力
            var input = (i == 0) ? 1.0f : 0.0f;
            resultsNoRes[i] = filterNoRes.TickAndRender(input, input, delta).left;
            resultsWithRes[i] = filterWithRes.TickAndRender(input, input, delta).left;
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
