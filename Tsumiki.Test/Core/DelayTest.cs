using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class DelayTest
{
    [Fact]
    public static void TickAndRender_出力値が有限()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 100,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, 44100);
        var delay = new Delay(config);

        for (var i = 0; i < 10000; i++)
        {
            var (left, right) = delay.TickAndRender(1.0f, 1.0f);
            Assert.False(float.IsNaN(left), $"サンプル {i} の左チャンネルで NaN が発生");
            Assert.False(float.IsInfinity(left), $"サンプル {i} の左チャンネルで無限大が発生");
            Assert.False(float.IsNaN(right), $"サンプル {i} の右チャンネルで NaN が発生");
            Assert.False(float.IsInfinity(right), $"サンプル {i} の右チャンネルで無限大が発生");
        }
    }

    [Fact]
    public static void TickAndRender_左右チャンネルが独立して動作する()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 10,
            Feedback = 0.0f,
            Cross = false, // クロスフィードバックなし
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, 44100);
        var delay = new Delay(config);
        var delaySamples = config.DelaySampleCount;

        // 左チャンネルのみにインパルスを入力
        var leftOutputs = new float[delaySamples * 2];
        var rightOutputs = new float[delaySamples * 2];

        for (var i = 0; i < leftOutputs.Length; i++)
        {
            var leftInput = (i == 0) ? 1.0f : 0.0f;
            var (left, right) = delay.TickAndRender(leftInput, 0.0f);
            leftOutputs[i] = left;
            rightOutputs[i] = right;
        }

        // 左チャンネルに信号が出力されることを確認
        var hasLeftSignal = false;
        for (var i = 0; i < leftOutputs.Length; i++)
        {
            if (MathF.Abs(leftOutputs[i]) > 0.01f)
            {
                hasLeftSignal = true;
                break;
            }
        }
        Assert.True(hasLeftSignal, "左チャンネルに信号が出力されていない");

        // 右チャンネルには信号が出力されないことを確認（クロスなし）
        for (var i = 0; i < rightOutputs.Length; i++)
        {
            Assert.InRange(MathF.Abs(rightOutputs[i]), 0f, 0.01f);
        }
    }

    [Fact]
    public static void TickAndRender_クロスフィードバックなしで正常に動作()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 100,
            Feedback = 0.5f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, 44100);
        var delay = new Delay(config);

        for (var i = 0; i < 1000; i++)
        {
            var (left, right) = delay.TickAndRender(1.0f, -1.0f);
            Assert.False(float.IsNaN(left));
            Assert.False(float.IsInfinity(left));
            Assert.False(float.IsNaN(right));
            Assert.False(float.IsInfinity(right));
        }
    }

    [Fact]
    public static void TickAndRender_クロスフィードバックありで正常に動作()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 100,
            Feedback = 0.5f,
            Cross = true, // クロスフィードバック有効
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, 44100);
        var delay = new Delay(config);

        for (var i = 0; i < 1000; i++)
        {
            var (left, right) = delay.TickAndRender(1.0f, -1.0f);
            Assert.False(float.IsNaN(left));
            Assert.False(float.IsInfinity(left));
            Assert.False(float.IsNaN(right));
            Assert.False(float.IsInfinity(right));
        }
    }

    [Fact]
    public static void TickAndRender_クロスフィードバックで左右が混ざる()
    {
        var unitNoCross = new DelayDelayUnit
        {
            Delay = 50, // 遅延時間を長くして、フィードバックが回るようにする
            Feedback = 0.7f, // フィードバックを高めにする
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var unitCross = new DelayDelayUnit
        {
            Delay = 50,
            Feedback = 0.7f,
            Cross = true,
            LowCut = 50,
            HighCut = 90
        };

        var configNoCross = new DelayConfig(unitNoCross, 44100);
        var configCross = new DelayConfig(unitCross, 44100);
        var delayNoCross = new Delay(configNoCross);
        var delayCross = new Delay(configCross);

        // 継続的に左チャンネルに入力を与える
        var samplesCount = 5000; // サンプル数を増やす
        var leftOutputsNoCross = new float[samplesCount];
        var rightOutputsNoCross = new float[samplesCount];
        var leftOutputsCross = new float[samplesCount];
        var rightOutputsCross = new float[samplesCount];

        for (var i = 0; i < samplesCount; i++)
        {
            // 継続的に入力を与える（最初の1000サンプル）
            var input = (i < 1000) ? 0.5f : 0.0f;
            var (l1, r1) = delayNoCross.TickAndRender(input, 0.0f);
            var (l2, r2) = delayCross.TickAndRender(input, 0.0f);

            leftOutputsNoCross[i] = l1;
            rightOutputsNoCross[i] = r1;
            leftOutputsCross[i] = l2;
            rightOutputsCross[i] = r2;
        }

        // クロスなしの場合、右チャンネルにはほとんど信号が出ない
        var rightEnergyNoCross = 0f;
        for (var i = 0; i < samplesCount; i++)
        {
            rightEnergyNoCross += MathF.Abs(rightOutputsNoCross[i]);
        }

        // クロスありの場合、右チャンネルにも信号が出る
        var rightEnergyCross = 0f;
        for (var i = 0; i < samplesCount; i++)
        {
            rightEnergyCross += MathF.Abs(rightOutputsCross[i]);
        }

        // クロスフィードバックがあると右チャンネルのエネルギーが明らかに増える
        Assert.True(rightEnergyCross > rightEnergyNoCross + 1.0f,
            $"クロスフィードバックで右チャンネルのエネルギーが増えていない (Cross: {rightEnergyCross}, NoCross: {rightEnergyNoCross})");
    }

    [Fact]
    public static void TickAndRender_フィードバック最大でも発散しない()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 100,
            Feedback = 1.0f,
            Cross = false,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, 44100);
        var delay = new Delay(config);

        for (var i = 0; i < 10000; i++)
        {
            var (left, right) = delay.TickAndRender(0.1f, 0.1f);
            Assert.False(float.IsNaN(left), $"サンプル {i} の左チャンネルで NaN が発生");
            Assert.False(float.IsInfinity(left), $"サンプル {i} の左チャンネルで無限大が発生");
            Assert.False(float.IsNaN(right), $"サンプル {i} の右チャンネルで NaN が発生");
            Assert.False(float.IsInfinity(right), $"サンプル {i} の右チャンネルで無限大が発生");
            Assert.InRange(left, -10f, 10f);
            Assert.InRange(right, -10f, 10f);
        }
    }

    [Fact]
    public static void TickAndRender_変動する入力でも安定している()
    {
        var unit = new DelayDelayUnit
        {
            Delay = 100,
            Feedback = 0.8f,
            Cross = true,
            LowCut = 50,
            HighCut = 90
        };
        var config = new DelayConfig(unit, 44100);
        var delay = new Delay(config);

        for (var i = 0; i < 10000; i++)
        {
            var leftInput = MathF.Sin(i * 0.01f);
            var rightInput = MathF.Cos(i * 0.01f);
            var (left, right) = delay.TickAndRender(leftInput, rightInput);
            Assert.False(float.IsNaN(left));
            Assert.False(float.IsInfinity(left));
            Assert.False(float.IsNaN(right));
            Assert.False(float.IsInfinity(right));
            Assert.InRange(left, -10f, 10f);
            Assert.InRange(right, -10f, 10f);
        }
    }
}
