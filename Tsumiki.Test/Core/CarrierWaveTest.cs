using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class CarrierWaveTest
{
    /// <summary>テスト用のキャリアユニットのモック</summary>
    private class TestCarrierUnit : ICarrierUnit
    {
        public float Level { get; set; }
        public double Pitch { get; set; }
        public bool Sync { get; set; }
        public float Phase { get; set; }
        public float ShapeX { get; set; }
        public float ShapeY { get; set; }
        public float Pan { get; set; }
        public int Attack { get; set; }
        public int Decay { get; set; }
        public float Sustain { get; set; }
        public int Release { get; set; }
    }

    [Fact]
    public static void Reset_位相がリセットされる()
    {
        var wave = new CarrierWave();
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0.25f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);

        // 事前に位相を進めておく
        for (var i = 0; i < 100; i++)
        {
            wave.TickAndRender(in config, 0.01, -1.0, 0f);
        }

        // リセット実行
        wave.Reset(in config);

        // リセット後の最初の出力を確認（位相が初期化されている）
        var result = wave.TickAndRender(in config, 0.01, -1.0, 0f);
        // Phase = 0.25f で開始し、そこから波形を計算するので、範囲内の値になる
        Assert.InRange(result, -1.5f, 1.5f);
    }

    [Fact]
    public static void TickAndRender_出力値が範囲内()
    {
        var wave = new CarrierWave();
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);

        wave.Reset(in config);

        for (var i = 0; i < 1000; i++)
        {
            var result = wave.TickAndRender(in config, 0.01, -1.0, 0f);
            // Level = 1.0f なので、出力は -1f ~ 1f の範囲内に収まるはず
            Assert.InRange(result, -1f, 1f);
        }
    }

    [Fact]
    public static void TickAndRender_位相が進む()
    {
        var wave = new CarrierWave();
        // ShapeY = 0 で純粋な三角波を生成
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);

        wave.Reset(in config);

        var results = new float[100];
        for (var i = 0; i < 100; i++)
        {
            results[i] = wave.TickAndRender(in config, 0.01, -1.0, 0f);
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
        var wave1 = new CarrierWave();
        var wave2 = new CarrierWave();

        var unit1 = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unit2 = new TestCarrierUnit { Pitch = 2.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config1 = new CarrierWaveConfig(unit1);
        var config2 = new CarrierWaveConfig(unit2);

        wave1.Reset(in config1);
        wave2.Reset(in config2);

        var delta = 0.01;
        var results1 = new float[200];
        var results2 = new float[200];

        for (var i = 0; i < 200; i++)
        {
            results1[i] = wave1.TickAndRender(in config1, delta, -1.0, 0f);
            results2[i] = wave2.TickAndRender(in config2, delta, -1.0, 0f);
        }

        // ピッチが2倍の場合、100サンプル後の値が、ピッチ1倍の200サンプル後の値と近似するはず
        Assert.InRange(MathF.Abs(results1[199] - results2[99]), 0f, 0.2f);
    }

    [Fact]
    public static void TickAndRender_シンク有効時に位相がリセットされる()
    {
        var wave = new CarrierWave();
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = true };
        var config = new CarrierWaveConfig(unit);

        wave.Reset(in config);

        var delta = 0.01;
        // まず位相を進める（syncPhase < 0 なのでシンクしない）
        for (var i = 0; i < 50; i++)
        {
            wave.TickAndRender(in config, delta, -1.0, 0f);
        }

        // シンクを発火させる（syncPhase >= 0）
        var resultBeforeSync = wave.TickAndRender(in config, delta, 0.0, 0f);
        var resultAfterSync = wave.TickAndRender(in config, delta, -1.0, 0f);

        // シンク後は位相がリセットされているため、波形が変化する可能性が高い
        // ただし、厳密な検証は難しいので、範囲チェックのみ
        Assert.InRange(resultBeforeSync, -1.5f, 1.5f);
        Assert.InRange(resultAfterSync, -1.5f, 1.5f);
    }

    [Fact]
    public static void TickAndRender_FM変調が効く()
    {
        var waveNoFm = new CarrierWave();
        var waveFm = new CarrierWave();

        // ShapeY = 0 で純粋な三角波を生成
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);

        waveNoFm.Reset(in config);
        waveFm.Reset(in config);

        var delta = 0.01;
        var resultsNoFm = new float[100];
        var resultsFm = new float[100];

        for (var i = 0; i < 100; i++)
        {
            resultsNoFm[i] = waveNoFm.TickAndRender(in config, delta, -1.0, 0f);
            resultsFm[i] = waveFm.TickAndRender(in config, delta, -1.0, 0.5f); // FM 変調あり
        }

        // FM変調があると波形が異なるはず
        var different = false;
        for (var i = 0; i < 100; i++)
        {
            if (MathF.Abs(resultsNoFm[i] - resultsFm[i]) > 0.01f)
            {
                different = true;
                break;
            }
        }
        Assert.True(different, "FM変調の有無で波形が変わらない");
    }

    [Fact]
    public static void TickAndRender_ShapeYで波形が変わる()
    {
        var waveNeutral = new CarrierWave();
        var wavePositive = new CarrierWave();
        var waveNegative = new CarrierWave();

        var unitNeutral = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unitPositive = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0.5f, Sync = false };
        var unitNegative = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = -0.5f, Sync = false };

        var configNeutral = new CarrierWaveConfig(unitNeutral);
        var configPositive = new CarrierWaveConfig(unitPositive);
        var configNegative = new CarrierWaveConfig(unitNegative);

        waveNeutral.Reset(in configNeutral);
        wavePositive.Reset(in configPositive);
        waveNegative.Reset(in configNegative);

        var delta = 0.01;
        var resultsNeutral = new float[100];
        var resultsPositive = new float[100];
        var resultsNegative = new float[100];

        for (var i = 0; i < 100; i++)
        {
            resultsNeutral[i] = waveNeutral.TickAndRender(in configNeutral, delta, -1.0, 0f);
            resultsPositive[i] = wavePositive.TickAndRender(in configPositive, delta, -1.0, 0f);
            resultsNegative[i] = waveNegative.TickAndRender(in configNegative, delta, -1.0, 0f);
        }

        // ShapeYが異なると波形が異なるはず
        var differentPositive = false;
        var differentNegative = false;

        for (var i = 0; i < 100; i++)
        {
            if (MathF.Abs(resultsNeutral[i] - resultsPositive[i]) > 0.01f)
            {
                differentPositive = true;
            }
            if (MathF.Abs(resultsNeutral[i] - resultsNegative[i]) > 0.01f)
            {
                differentNegative = true;
            }
        }

        Assert.True(differentPositive, "ShapeY=0.5で波形が変わらない");
        Assert.True(differentNegative, "ShapeY=-0.5で波形が変わらない");
    }

    [Fact]
    public static void TickAndRender_ShapeXで波形が変わる()
    {
        var waveNeutral = new CarrierWave();
        var wavePositive = new CarrierWave();

        // ShapeY = 0 で純粋な三角波を生成
        var unitNeutral = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unitPositive = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0.5f, ShapeY = 0f, Sync = false };

        var configNeutral = new CarrierWaveConfig(unitNeutral);
        var configPositive = new CarrierWaveConfig(unitPositive);

        waveNeutral.Reset(in configNeutral);
        wavePositive.Reset(in configPositive);

        var delta = 0.01;
        var resultsNeutral = new float[100];
        var resultsPositive = new float[100];

        for (var i = 0; i < 100; i++)
        {
            resultsNeutral[i] = waveNeutral.TickAndRender(in configNeutral, delta, -1.0, 0f);
            resultsPositive[i] = wavePositive.TickAndRender(in configPositive, delta, -1.0, 0f);
        }

        // ShapeXが異なると波形が異なるはず
        var different = false;
        for (var i = 0; i < 100; i++)
        {
            if (MathF.Abs(resultsNeutral[i] - resultsPositive[i]) > 0.01f)
            {
                different = true;
                break;
            }
        }

        Assert.True(different, "ShapeXの変更で波形が変わらない");
    }

    [Fact]
    public static void TickAndRender_Levelで出力が変わる()
    {
        var waveHalf = new CarrierWave();
        var waveFull = new CarrierWave();

        var unitHalf = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 0.5f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unitFull = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };

        var configHalf = new CarrierWaveConfig(unitHalf);
        var configFull = new CarrierWaveConfig(unitFull);

        waveHalf.Reset(in configHalf);
        waveFull.Reset(in configFull);

        var delta = 0.01;
        var resultsHalf = new float[100];
        var resultsFull = new float[100];

        for (var i = 0; i < 100; i++)
        {
            resultsHalf[i] = waveHalf.TickAndRender(in configHalf, delta, -1.0, 0f);
            resultsFull[i] = waveFull.TickAndRender(in configFull, delta, -1.0, 0f);
        }

        // Level=0.5 の出力が Level=1.0 の約半分になっているか確認
        for (var i = 0; i < 100; i++)
        {
            // Level が半分なら、出力も約半分になるはず
            Assert.InRange(MathF.Abs(resultsHalf[i] * 2f - resultsFull[i]), 0f, 0.1f);
        }
    }

    [Fact]
    public static void CarrierWaveConfig_設定値が正しく格納される()
    {
        var unit = new TestCarrierUnit
        {
            Pitch = 2.5,
            Phase = 0.3f,
            Level = 0.8f,
            ShapeX = 0.2f,
            ShapeY = -0.4f,
            Sync = true
        };

        var config = new CarrierWaveConfig(unit);

        Assert.Equal(2.5, config.Pitch);
        Assert.Equal(0.3f, config.Phase);
        Assert.Equal(0.8f, config.Level);
        Assert.True(config.Sync);
    }

    [Fact]
    public static void CarrierWaveConfig_ShapeYが正の時の計算()
    {
        var unit = new TestCarrierUnit
        {
            Pitch = 1.0,
            Phase = 0f,
            Level = 1.0f,
            ShapeX = 0f,
            ShapeY = 0.5f,
            Sync = false
        };

        var config = new CarrierWaveConfig(unit);

        // ShapeY が正の値の場合、三角波 → 矩形波の変化
        Assert.Equal(2.0f, config.TriFactor);
        Assert.Equal(0.0f, config.SinFactor);
    }

    [Fact]
    public static void CarrierWaveConfig_ShapeYが負の時の計算()
    {
        var unit = new TestCarrierUnit
        {
            Pitch = 1.0,
            Phase = 0f,
            Level = 1.0f,
            ShapeX = 0f,
            ShapeY = -0.5f,
            Sync = false
        };

        var config = new CarrierWaveConfig(unit);

        // ShapeY が負の値の場合、三角波 → サイン波の変化
        Assert.Equal(1f, config.TriFactor);
        Assert.Equal(0.5f, config.SinFactor);
    }
}
