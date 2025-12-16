using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class DelayConfigTest
{
    [Fact]
    public static void DelayConfig_通常のパラメータで正常に動作()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 250,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        // DelaySampleCount が正しく計算されていることを確認
        var expectedSamples = (int)(250 * 44100 / 1000);
        Assert.Equal(expectedSamples, config.DelaySampleCount);
        Assert.Equal(0.5f, config.Feedback);
        Assert.False(config.Cross);
    }

    [Fact]
    public static void DelayConfig_最小ディレイ時間でも正常に動作()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 2,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        // DelaySampleCount が正の値であることを確認
        Assert.True(config.DelaySampleCount > 0, "最小ディレイ時間でもサンプル数が正の値になっている");
        Assert.False(float.IsNaN(config.Feedback));
        Assert.False(float.IsInfinity(config.Feedback));
    }

    [Fact]
    public static void DelayConfig_最大ディレイ時間でも正常に動作()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 500,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        var expectedSamples = (int)(500 * 44100 / 1000);
        Assert.Equal(expectedSamples, config.DelaySampleCount);
        Assert.True(config.DelaySampleCount > 0);
    }

    [Fact]
    public static void DelayConfig_フィードバック0でも正常に動作()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 250,
            Feedback = 0.0f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        Assert.Equal(0.0f, config.Feedback);
        Assert.False(float.IsNaN(config.Feedback));
    }

    [Fact]
    public static void DelayConfig_フィードバック1でも正常に動作()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 250,
            Feedback = 1.0f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        Assert.Equal(1.0f, config.Feedback);
        Assert.False(float.IsNaN(config.Feedback));
    }

    [Fact]
    public static void DelayConfig_クロスフィードバックが反映される()
    {
        var unitNoCross = new DelayDelayUnit
        {
            Delay = 250,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var unitCross = new DelayDelayUnit
        {
            Delay = 250,
            Feedback = 0.5f,
            Cross = true,
            LowCut = 50,
            HighCut = 90
        };

        var configNoCross = new DelayConfig(unitNoCross, sampleRate: 44100);
        var configCross = new DelayConfig(unitCross, sampleRate: 44100);

        Assert.False(configNoCross.Cross);
        Assert.True(configCross.Cross);
    }

    [Fact]
    public static void DelayConfig_フィルター設定が反映される()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 250,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        // フィルター設定が有効な値であることを確認
        Assert.InRange(config.LowCutConfig.Alpha, 0f, 1f);
        Assert.InRange(config.HighCutConfig.Alpha, 0f, 1f);
        Assert.False(float.IsNaN(config.LowCutConfig.Alpha));
        Assert.False(float.IsNaN(config.HighCutConfig.Alpha));
    }
}
