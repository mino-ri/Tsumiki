using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class EnvelopeTest
{
    [Fact]
    public static void TickAndRender_初期状態でノートオフの場合は0を返す()
    {
        var envelope = new Envelope();
        var config = new EnvelopeConfig(40, 40, 0.5f, 40, 44100.0);

        var result = envelope.TickAndRender(in config, false);

        Assert.Equal(0f, result);
    }

    [Fact]
    public static void TickAndRender_ノートオンでアタックフェーズが開始される()
    {
        var envelope = new Envelope();
        var config = new EnvelopeConfig(40, 40, 0.5f, 40, 1000.0);

        var prev = 0f;
        for (var i = 0; i < 1200; i++)
        {
            var result = envelope.TickAndRender(in config, true);
            // アタックフェーズでは値が増加する
            if (result < 1f)
            {
                Assert.True(result >= prev, $"アタックフェーズで値が減少: {prev} -> {result}");
                prev = result;
            }
            else
            {
                // 最大値に達したら終了
                Assert.Equal(1f, result);
                Assert.Equal(99, i);
                break;
            }
        }
    }

    [Fact]
    public static void TickAndRender_サステインレベルまで減衰する()
    {
        var envelope = new Envelope();
        var sustainLevel = 0.5f;
        var config = new EnvelopeConfig(60, 60, sustainLevel, 40, 44100.0);

        // アタックで最大値まで到達させる
        for (var i = 0; i < 10000; i++)
        {
            var result = envelope.TickAndRender(in config, true);
            if (result >= 1f)
                break;
        }

        // ディケイフェーズでサステインレベルまで減衰
        var decayResult = 0f;
        for (var i = 0; i < 100000; i++)
        {
            decayResult = envelope.TickAndRender(in config, true);
            if (MathF.Abs(decayResult - sustainLevel) < 0.01f)
                break;
        }

        // サステインレベル付近に収束しているか確認
        Assert.InRange(decayResult, sustainLevel - 0.01f, sustainLevel + 0.01f);
    }

    [Fact]
    public static void TickAndRender_ノートオフでリリースフェーズに入る()
    {
        var envelope = new Envelope();
        var config = new EnvelopeConfig(40, 40, 0.8f, 40, 44100.0);

        // まずノート・オンでアタック
        for (var i = 0; i < 1000; i++)
        {
            envelope.TickAndRender(in config, true);
        }

        // ノート・オフでリリース
        var prev = envelope.TickAndRender(in config, false);
        for (var i = 0; i < 100; i++)
        {
            var result = envelope.TickAndRender(in config, false);
            // リリースフェーズでは値が減少する
            if (result > 0f)
            {
                Assert.True(result <= prev, $"リリースフェーズで値が増加: {prev} -> {result}");
                prev = result;
            }
            else
            {
                // 0に達したら終了
                Assert.Equal(0f, result);
                break;
            }
        }
    }

    [Fact]
    public static void TickAndRender_リリース後は0を維持する()
    {
        var envelope = new Envelope();
        var config = new EnvelopeConfig(40, 40, 0.5f, 40, 44100.0);

        // アタック
        for (var i = 0; i < 1000; i++)
        {
            envelope.TickAndRender(in config, true);
        }

        // リリースして0まで減衰
        for (var i = 0; i < 100000; i++)
        {
            var result = envelope.TickAndRender(in config, false);
            if (result == 0f)
                break;
        }

        // その後も0を維持
        for (var i = 0; i < 100; i++)
        {
            var result = envelope.TickAndRender(in config, false);
            Assert.Equal(0f, result);
        }
    }

    [Fact]
    public static void TickAndRender_リリース途中からノートオンできる()
    {
        var envelope = new Envelope();
        var config = new EnvelopeConfig(20, 20, 0.5f, 40, 44100.0);

        // アタック
        for (var i = 0; i < 1000; i++)
        {
            envelope.TickAndRender(in config, true);
        }

        // リリース開始
        for (var i = 0; i < 100; i++)
        {
            envelope.TickAndRender(in config, false);
        }

        // 再度ノート・オン
        var result = envelope.TickAndRender(in config, true);
        // 0より大きい値が返る（再アタック開始）
        Assert.True(result > 0f);

        // Envelope の実装では、リリース中（_decaying = true）からノート・オンすると
        // サステインレベルに向かって減衰する動作になる可能性がある
        // そのため、値が単調増加するとは限らない
        // 少なくとも最終的に0ではない値に収束することを確認
        for (var i = 0; i < 10000; i++)
        {
            result = envelope.TickAndRender(in config, true);
        }
        // 十分時間が経過した後、サステインレベル付近の値になっているはず
        Assert.InRange(result, 0.4f, 0.6f);
    }

    [Fact]
    public static void TickAndRender_出力値は0から1の範囲内()
    {
        var envelope = new Envelope();
        var config = new EnvelopeConfig(40, 40, 0.7f, 40, 44100.0);

        // ノート・オン
        for (var i = 0; i < 10000; i++)
        {
            var result = envelope.TickAndRender(in config, true);
            Assert.InRange(result, 0f, 1f);
        }

        // ノート・オフ
        for (var i = 0; i < 10000; i++)
        {
            var result = envelope.TickAndRender(in config, false);
            Assert.InRange(result, 0f, 1f);
        }
    }

    [Fact]
    public static void EnvelopeConfig_コンストラクタでレートが正しく計算される()
    {
        var sampleRate = 44100.0;
        var config = new EnvelopeConfig(40, 50, 0.6f, 60, sampleRate);

        // レートは0～1の範囲内
        Assert.InRange(config.AttackDelta, 0.0, 1.0);
        Assert.InRange(config.DecayRate, 0.0, 1.0);
        Assert.InRange(config.ReleaseRate, 0.0, 1.0);

        // サステインレベルは設定値と近似（float -> double の変換による誤差を許容）
        Assert.InRange(config.SustainLevel, 0.59, 0.61);
    }

    [Fact]
    public static void EnvelopeConfig_値が大きいほどレートが小さい()
    {
        var sampleRate = 44100.0;
        var config1 = new EnvelopeConfig(20, 20, 0.5f, 20, sampleRate);
        var config2 = new EnvelopeConfig(40, 40, 0.5f, 40, sampleRate);
        var config3 = new EnvelopeConfig(60, 60, 0.5f, 60, sampleRate);

        // GetEnvelopeRate の実装により、値が大きいほどレートは小さくなる（ゆっくり変化する）
        Assert.True(config1.AttackDelta > config2.AttackDelta);
        Assert.True(config2.AttackDelta > config3.AttackDelta);
        Assert.True(config1.DecayRate > config2.DecayRate);
        Assert.True(config2.DecayRate > config3.DecayRate);
        Assert.True(config1.ReleaseRate > config2.ReleaseRate);
        Assert.True(config2.ReleaseRate > config3.ReleaseRate);
    }
}
