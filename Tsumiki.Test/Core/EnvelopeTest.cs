using IndirectX;
using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class EnvelopeTest
{
    [Fact]
    public static void TickAndRender_初期状態でノートオフの場合は0を返す()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 40, Sustain = 0.5f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        var result = envelope.TickAndRender(false);

        Assert.Equal(0f, result);
    }

    [Fact]
    public static void TickAndRender_ノートオンでアタックフェーズが開始される()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 40, Sustain = 0.5f, Release = 40 };
        var config = new EnvelopeConfig(unit, 1000.0);
        var envelope = new Envelope(config);

        var prev = 0f;
        for (var i = 0; i < 1200; i++)
        {
            var result = envelope.TickAndRender(true);
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
        var sustainLevel = 0.5f;
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 60, Decay = 60, Sustain = sustainLevel, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        // アタックで最大値まで到達させる
        for (var i = 0; i < 10000; i++)
        {
            var result = envelope.TickAndRender(true);
            if (result >= 1f)
                break;
        }

        // ディケイフェーズでサステインレベルまで減衰
        var decayResult = 0f;
        for (var i = 0; i < 100000; i++)
        {
            decayResult = envelope.TickAndRender(true);
            if (MathF.Abs(decayResult - sustainLevel) < 0.01f)
                break;
        }

        // サステインレベル付近に収束しているか確認
        Assert.InRange(decayResult, sustainLevel - 0.01f, sustainLevel + 0.01f);
    }

    [Fact]
    public static void TickAndRender_ノートオフでリリースフェーズに入る()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 40, Sustain = 0.8f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        // まずノート・オンでアタック
        for (var i = 0; i < 1000; i++)
        {
            envelope.TickAndRender(true);
        }

        // ノート・オフでリリース
        var prev = envelope.TickAndRender(false);
        for (var i = 0; i < 100; i++)
        {
            var result = envelope.TickAndRender(false);
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
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 40, Sustain = 0.5f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        // アタック
        for (var i = 0; i < 1000; i++)
        {
            var result = envelope.TickAndRender(true);
        }

        // リリースして0まで減衰
        for (var i = 0; i < 100000; i++)
        {
            var result = envelope.TickAndRender(false);
            if (result == 0f)
                break;
        }

        // その後も0を維持
        for (var i = 0; i < 100; i++)
        {
            var result = envelope.TickAndRender(false);
            Assert.Equal(0f, result);
        }
    }

    [Fact]
    public static void TickAndRender_リリース途中からノートオンできる()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 20, Decay = 20, Sustain = 0.5f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        // アタック
        for (var i = 0; i < 1000; i++)
        {
            envelope.TickAndRender(true);
        }

        // リリース開始
        for (var i = 0; i < 100; i++)
        {
            envelope.TickAndRender(false);
        }

        // 再度ノート・オン
        var result = envelope.TickAndRender(true);
        // 0より大きい値が返る（再アタック開始）
        Assert.True(result > 0f);

        // Envelope の実装では、リリース中（_decaying = true）からノート・オンすると
        // サステインレベルに向かって減衰する動作になる可能性がある
        // そのため、値が単調増加するとは限らない
        // 少なくとも最終的に0ではない値に収束することを確認
        for (var i = 0; i < 10000; i++)
        {
            result = envelope.TickAndRender(true);
        }
        // 十分時間が経過した後、サステインレベル付近の値になっているはず
        Assert.InRange(result, 0.4f, 0.6f);
    }

    [Fact]
    public static void TickAndRender_出力値は0から1の範囲内()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 40, Sustain = 0.7f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        // ノート・オン
        for (var i = 0; i < 10000; i++)
        {
            var result = envelope.TickAndRender(true);
            Assert.InRange(result, 0f, 1f);
        }

        // ノート・オフ
        for (var i = 0; i < 10000; i++)
        {
            var result = envelope.TickAndRender(false);
            Assert.InRange(result, 0f, 1f);
        }
    }

    [Fact]
    public static void EnvelopeConfig_コンストラクタでレートが正しく計算される()
    {
        var sampleRate = 44100.0;
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 50, Sustain = 0.6f, Release = 60 };
        var config = new EnvelopeConfig(unit, sampleRate);

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
        var unit1 = new EnvelopeModulationEnvelopeUnit { Attack = 20, Decay = 20, Sustain = 0.5f, Release = 20 };
        var unit2 = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 40, Sustain = 0.5f, Release = 40 };
        var unit3 = new EnvelopeModulationEnvelopeUnit { Attack = 60, Decay = 60, Sustain = 0.5f, Release = 60 };
        var config1 = new EnvelopeConfig(unit1, sampleRate);
        var config2 = new EnvelopeConfig(unit2, sampleRate);
        var config3 = new EnvelopeConfig(unit3, sampleRate);

        // GetEnvelopeRate の実装により、値が大きいほどレートは小さくなる（ゆっくり変化する）
        Assert.True(config1.AttackDelta > config2.AttackDelta);
        Assert.True(config2.AttackDelta > config3.AttackDelta);
        Assert.True(config1.DecayRate > config2.DecayRate);
        Assert.True(config2.DecayRate > config3.DecayRate);
        Assert.True(config1.ReleaseRate > config2.ReleaseRate);
        Assert.True(config2.ReleaseRate > config3.ReleaseRate);
    }

    [Fact]
    public static void EnvelopeConfig_Recalculate_サンプルレート変更で再計算される()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 40, Sustain = 0.5f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);

        var oldAttackDelta = config.AttackDelta;
        var oldDecayRate = config.DecayRate;
        var oldReleaseRate = config.ReleaseRate;

        // サンプルレートを変更して再計算
        config.Recalculate(48000.0);

        // レートが変わることを確認（サンプルレートが変わると計算結果も変わる）
        Assert.NotEqual(oldAttackDelta, config.AttackDelta);
        Assert.NotEqual(oldDecayRate, config.DecayRate);
        Assert.NotEqual(oldReleaseRate, config.ReleaseRate);
    }

    [Fact]
    public static void EnvelopeConfig_Recalculate_パラメータ変更で再計算される()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 40, Sustain = 0.5f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);

        var oldAttackDelta = config.AttackDelta;
        var oldDecayRate = config.DecayRate;
        var oldSustainLevel = config.SustainLevel;

        // パラメータを変更
        unit.Attack = 60;
        unit.Decay = 60;
        unit.Sustain = 0.7f;
        unit.Release = 60;

        config.Recalculate(44100.0);

        // 変更が反映されることを確認
        Assert.NotEqual(oldAttackDelta, config.AttackDelta);
        Assert.NotEqual(oldDecayRate, config.DecayRate);
        Assert.NotEqual(oldSustainLevel, config.SustainLevel);
        Assert.InRange(config.SustainLevel, 0.69, 0.71);
    }

    [Fact]
    public static void Envelope_Restart_減衰をリセットする()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 10, Decay = 10, Sustain = 0.5f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        // アタックからディケイまで進める
        for (var i = 0; i < 2000; i++)
        {
            envelope.TickAndRender(true);
        }

        // Restart を呼ぶ
        envelope.Restart();

        // 再度ノートオンすると、アタックフェーズから開始される
        var result = envelope.TickAndRender(true);
        Assert.True(result < 1f); // まだ最大値に達していない
    }

    [Fact]
    public static void Envelope_Reset_完全に初期状態に戻る()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 40, Decay = 40, Sustain = 0.5f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        // アタック状態まで進める
        for (var i = 0; i < 100; i++)
        {
            envelope.TickAndRender(true);
        }

        // Reset を呼ぶ
        envelope.Reset();

        // ノートオフ状態で呼んでも 0 が返る（完全にリセットされている）
        var result = envelope.TickAndRender(false);
        Assert.Equal(0f, result);
    }

    [Fact]
    public static void TickAndRender_Attack値が小さいほど速く最大値に達する()
    {
        var unitSlow = new EnvelopeModulationEnvelopeUnit { Attack = 60, Decay = 40, Sustain = 0.5f, Release = 40 };
        var unitFast = new EnvelopeModulationEnvelopeUnit { Attack = 10, Decay = 40, Sustain = 0.5f, Release = 40 };
        var configSlow = new EnvelopeConfig(unitSlow, 44100.0);
        var configFast = new EnvelopeConfig(unitFast, 44100.0);
        var envelopeSlow = new Envelope(configSlow);
        var envelopeFast = new Envelope(configFast);

        // Attack値が小さい方が速く最大値に達する
        var slowIterations = 0;
        var fastIterations = 0;

        for (var i = 1; i <= 50000; i++)
        {
            var resultSlow = envelopeSlow.TickAndRender(true);
            if (slowIterations == 0 && resultSlow >= 0.99f)
            {
                slowIterations = i;
            }

            var resultFast = envelopeFast.TickAndRender(true);
            if (fastIterations == 0 && resultFast >= 0.99f)
            {
                fastIterations = i;
            }

            if (slowIterations > 0 && fastIterations > 0)
                break;
        }

        Assert.True(fastIterations > 0, "Fast envelope should reach max");
        Assert.True(slowIterations > 0, "Slow envelope should reach max");
        Assert.True(fastIterations < slowIterations, $"Attack値が小さい方が速く最大値に達する (fast: {fastIterations}, slow: {slowIterations})");
    }

    [Fact]
    public static void TickAndRender_Release値が小さいほど速く0に達する()
    {
        var unitSlow = new EnvelopeModulationEnvelopeUnit { Attack = 20, Decay = 20, Sustain = 0.5f, Release = 60 };
        var unitFast = new EnvelopeModulationEnvelopeUnit { Attack = 20, Decay = 20, Sustain = 0.5f, Release = 10 };
        var configSlow = new EnvelopeConfig(unitSlow, 44100.0);
        var configFast = new EnvelopeConfig(unitFast, 44100.0);
        var envelopeSlow = new Envelope(configSlow);
        var envelopeFast = new Envelope(configFast);

        // アタック状態まで進める
        for (var i = 0; i < 2000; i++)
        {
            envelopeSlow.TickAndRender(true);
            envelopeFast.TickAndRender(true);
        }

        // Release値が小さい方が速く0に達する
        var slowIterations = 0;
        var fastIterations = 0;

        for (var i = 0; i < 100000; i++)
        {
            var resultSlow = envelopeSlow.TickAndRender(false);
            if (slowIterations == 0 && resultSlow <= 0.01f)
            {
                slowIterations = i;
            }

            var resultFast = envelopeFast.TickAndRender(false);
            if (fastIterations == 0 && resultFast <= 0.01f)
            {
                fastIterations = i;
            }

            if (slowIterations > 0 && fastIterations > 0)
                break;
        }

        Assert.True(fastIterations < slowIterations, $"Release値が小さい方が速く0に達する (fast: {fastIterations}, slow: {slowIterations})");
    }

    [Fact]
    public static void TickAndRender_Sustain0でディケイ後は無音()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 20, Decay = 20, Sustain = 0f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        // アタックとディケイを完了させる
        for (var i = 0; i < 5000; i++)
        {
            envelope.TickAndRender(true);
        }

        // サステインレベルが 0 なので出力は 0
        var result = envelope.TickAndRender(true);
        Assert.InRange(result, 0f, 0.01f);
    }

    [Fact]
    public static void TickAndRender_Sustain1でディケイなし()
    {
        var unit = new EnvelopeModulationEnvelopeUnit { Attack = 20, Decay = 20, Sustain = 1f, Release = 40 };
        var config = new EnvelopeConfig(unit, 44100.0);
        var envelope = new Envelope(config);

        // アタック完了まで進める
        for (var i = 0; i < 1000; i++)
        {
            var result = envelope.TickAndRender(true);
            if (result >= 1f)
                break;
        }

        // サステインが 1 なのでディケイせず、1 のまま
        for (var i = 0; i < 1000; i++)
        {
            var result = envelope.TickAndRender(true);
            Assert.Equal(1f, result);
        }
    }
}
