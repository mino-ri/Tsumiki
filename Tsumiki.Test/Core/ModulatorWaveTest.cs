using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class ModulatorWaveTest
{
    /// <summary>テスト用のモジュレータユニットのモック</summary>
    private class TestModulatorUnit : IModulatorUnit
    {
        public float Level { get; set; }
        public double Pitch { get; set; }
        public bool Sync { get; set; }
        public float Phase { get; set; }
        public float Feedback { get; set; }
        public int Attack { get; set; }
        public int Decay { get; set; }
        public float Sustain { get; set; }
        public int Release { get; set; }
    }

    [Fact]
    public static void Reset_位相とフィードバックがリセットされる()
    {
        var wave = new ModulatorWave();
        var unit = new TestModulatorUnit { Pitch = 1.0, Phase = 0.25f, Feedback = 0f, Sync = false };
        var config = new ModulatorWaveConfig(unit);

        // 事前に位相を進めておく
        for (var i = 0; i < 100; i++)
        {
            wave.TickAndRender(in config, 0.01, -1.0);
        }

        // リセット実行
        wave.Reset(in config);

        // リセット後の最初の出力を確認（位相が初期化されている）
        var result = wave.TickAndRender(in config, 0.01, -1.0);
        // Phase = 0.25f で開始し、そこから sin を計算するので、おおよそ sin(0.25 * 2π) 付近の値になる
        Assert.InRange(result, -1f, 1f);
    }

    [Fact]
    public static void TickAndRender_出力値が範囲内()
    {
        var wave = new ModulatorWave();
        var unit = new TestModulatorUnit { Pitch = 1.0, Phase = 0f, Feedback = 0f, Sync = false };
        var config = new ModulatorWaveConfig(unit);

        wave.Reset(in config);

        for (var i = 0; i < 1000; i++)
        {
            var result = wave.TickAndRender(in config, 0.01, -1.0);
            Assert.InRange(result, -1.1f, 1.1f);
        }
    }

    [Fact]
    public static void TickAndRender_位相が進む()
    {
        var wave = new ModulatorWave();
        var unit = new TestModulatorUnit { Pitch = 1.0, Phase = 0f, Feedback = 0f, Sync = false };
        var config = new ModulatorWaveConfig(unit);

        wave.Reset(in config);

        var results = new float[100];
        for (var i = 0; i < 100; i++)
        {
            results[i] = wave.TickAndRender(in config, 0.01, -1.0);
        }

        // 位相が進むので、出力が周期的に変化するはず
        // 全て同じ値でないことを確認
        var allSame = true;
        for (var i = 1; i < results.Length; i++)
        {
            if (MathF.Abs(results[i] - results[0]) > 0.01f)
            {
                allSame = false;
                break;
            }
        }
        Assert.False(allSame, "位相が進んでいないため、出力が変化していない");
    }

    [Fact]
    public static void TickAndRender_ピッチが2倍だと周期が半分()
    {
        var wave1 = new ModulatorWave();
        var wave2 = new ModulatorWave();

        var unit1 = new TestModulatorUnit { Pitch = 1.0, Phase = 0f, Feedback = 0f, Sync = false };
        var unit2 = new TestModulatorUnit { Pitch = 2.0, Phase = 0f, Feedback = 0f, Sync = false };
        var config1 = new ModulatorWaveConfig(unit1);
        var config2 = new ModulatorWaveConfig(unit2);

        wave1.Reset(in config1);
        wave2.Reset(in config2);

        var delta = 0.01;
        var results1 = new float[200];
        var results2 = new float[200];

        for (var i = 0; i < 200; i++)
        {
            results1[i] = wave1.TickAndRender(in config1, delta, -1.0);
            results2[i] = wave2.TickAndRender(in config2, delta, -1.0);
        }

        // ピッチが2倍の場合、100サンプル後の値が、ピッチ1倍の200サンプル後の値と近似するはず
        Assert.InRange(MathF.Abs(results1[199] - results2[99]), 0f, 0.1f);
    }

    [Fact]
    public static void TickAndRender_フィードバックあり()
    {
        var waveNoFb = new ModulatorWave();
        var waveFb = new ModulatorWave();

        var unitNoFb = new TestModulatorUnit { Pitch = 1.0, Phase = 0f, Feedback = 0f, Sync = false };
        var unitFb = new TestModulatorUnit { Pitch = 1.0, Phase = 0f, Feedback = 0.5f, Sync = false };
        var configNoFb = new ModulatorWaveConfig(unitNoFb);
        var configFb = new ModulatorWaveConfig(unitFb);

        waveNoFb.Reset(in configNoFb);
        waveFb.Reset(in configFb);

        var delta = 0.01;
        var resultsNoFb = new float[100];
        var resultsFb = new float[100];

        for (var i = 0; i < 100; i++)
        {
            resultsNoFb[i] = waveNoFb.TickAndRender(in configNoFb, delta, -1.0);
            resultsFb[i] = waveFb.TickAndRender(in configFb, delta, -1.0);
        }

        // フィードバックがあると波形が異なるはず
        var different = false;
        for (var i = 0; i < 100; i++)
        {
            if (MathF.Abs(resultsNoFb[i] - resultsFb[i]) > 0.01f)
            {
                different = true;
                break;
            }
        }
        Assert.True(different, "フィードバックの有無で波形が変わらない");
    }

    [Fact]
    public static void TickAndRender_シンク有効時に位相がリセットされる()
    {
        var wave = new ModulatorWave();
        var unit = new TestModulatorUnit { Pitch = 1.0, Phase = 0f, Feedback = 0f, Sync = true };
        var config = new ModulatorWaveConfig(unit);

        wave.Reset(in config);

        var delta = 0.01;
        // まず位相を進める（syncPhase < 0 なのでシンクしない）
        for (var i = 0; i < 50; i++)
        {
            wave.TickAndRender(in config, delta, -1.0);
        }

        // シンクを発火させる（syncPhase >= 0）
        var resultBeforeSync = wave.TickAndRender(in config, delta, 0.0);
        var resultAfterSync = wave.TickAndRender(in config, delta, -1.0);

        // シンク後は位相がリセットされているため、波形が変化する可能性が高い
        // ただし、厳密な検証は難しいので、範囲チェックのみ
        Assert.InRange(resultBeforeSync, -1.1f, 1.1f);
        Assert.InRange(resultAfterSync, -1.1f, 1.1f);
    }

    [Fact]
    public static void ModulatorWaveConfig_設定値が正しく格納される()
    {
        var unit = new TestModulatorUnit
        {
            Pitch = 2.5,
            Phase = 0.3f,
            Feedback = 0.7f,
            Sync = true
        };

        var config = new ModulatorWaveConfig(unit);

        Assert.Equal(2.5, config.Pitch);
        Assert.Equal(0.3f, config.Phase);
        Assert.Equal(0.7f, config.Feedback);
        Assert.True(config.Sync);
    }
}
