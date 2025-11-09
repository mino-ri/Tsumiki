using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class DelayChannelTest
{
    /// <summary>テスト用のディレイユニットのモック</summary>
    private class TestDelayUnit : IDelayUnit
    {
        public float Mix { get; set; }
        public int Delay { get; set; }
        public float Feedback { get; set; }
        public bool Cross { get; set; }
        public int LowCut { get; set; }
        public int HighCut { get; set; }
    }

    [Fact]
    public static void TickAndRender_出力値が有限()
    {
        var channel = new DelayChannel(sampleRate: 44100);
        var unit = new TestDelayUnit
        {
            Delay = 100,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        for (var i = 0; i < 10000; i++)
        {
            var output = channel.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_遅延が正しく機能する()
    {
        var channel = new DelayChannel(sampleRate: 44100);
        var unit = new TestDelayUnit
        {
            Delay = 10, // 10ms の遅延
            Feedback = 0.0f, // フィードバックなし
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);
        var delaySamples = config.DelaySampleCount;

        // インパルス入力を与える
        var outputs = new float[delaySamples * 2];
        for (var i = 0; i < outputs.Length; i++)
        {
            var input = (i == 0) ? 1.0f : 0.0f;
            outputs[i] = channel.TickAndRender(in config, input);
        }

        // 最初の出力は0であるべき（遅延があるため）
        Assert.InRange(MathF.Abs(outputs[0]), 0f, 0.01f);

        // delaySamples 後に信号が出てくるべき
        // フィルタの影響で多少減衰する可能性があるため、範囲チェック
        Assert.True(MathF.Abs(outputs[delaySamples]) > 0.01f, $"遅延後に信号が検出されない: {outputs[delaySamples]}");
    }

    [Fact]
    public static void TickAndRender_フィードバック0で発散しない()
    {
        var channel = new DelayChannel(sampleRate: 44100);
        var unit = new TestDelayUnit
        {
            Delay = 100,
            Feedback = 0.0f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        for (var i = 0; i < 10000; i++)
        {
            var output = channel.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }

        // フィードバックが0になっていることを確認
        Assert.Equal(0.0f, channel.Feedback);
    }

    [Fact]
    public static void TickAndRender_フィードバック1でも発散しない()
    {
        var channel = new DelayChannel(sampleRate: 44100);
        var unit = new TestDelayUnit
        {
            Delay = 100,
            Feedback = 1.0f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        // フィードバック1.0の場合、理論上は発散する可能性があるが、
        // フィルタによる減衰とフィードバック閾値処理により安定するはず
        for (var i = 0; i < 10000; i++)
        {
            var output = channel.TickAndRender(in config, input: 0.1f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
            Assert.InRange(output, -10f, 10f); // 有限範囲に収まることを確認
        }
    }

    [Fact]
    public static void TickAndRender_フィードバック閾値処理が機能する()
    {
        var channel = new DelayChannel(sampleRate: 44100);
        var unit = new TestDelayUnit
        {
            Delay = 100,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        // 微小な入力を与え続けると、フィードバックが閾値以下になって0になるはず
        for (var i = 0; i < 50000; i++)
        {
            channel.TickAndRender(in config, input: 0.0f);
        }

        // 長時間経過後、フィードバックは0になるべき
        Assert.Equal(0.0f, channel.Feedback);
    }

    [Fact]
    public static void TickAndRender_最小ディレイ時間でも正常に動作()
    {
        var channel = new DelayChannel(sampleRate: 44100);
        var unit = new TestDelayUnit
        {
            Delay = 2,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = channel.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_最大ディレイ時間でも正常に動作()
    {
        var channel = new DelayChannel(sampleRate: 44100);
        var unit = new TestDelayUnit
        {
            Delay = 500,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        for (var i = 0; i < 1000; i++)
        {
            var output = channel.TickAndRender(in config, input: 1.0f);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_変動する入力でも安定している()
    {
        var channel = new DelayChannel(sampleRate: 44100);
        var unit = new TestDelayUnit
        {
            Delay = 100,
            Feedback = 0.8f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, sampleRate: 44100);

        for (var i = 0; i < 10000; i++)
        {
            // サイン波入力
            var input = MathF.Sin(i * 0.01f);
            var output = channel.TickAndRender(in config, input);
            Assert.False(float.IsNaN(output), $"サンプル {i} で NaN が発生");
            Assert.False(float.IsInfinity(output), $"サンプル {i} で無限大が発生");
            Assert.InRange(output, -10f, 10f);
        }
    }
}
