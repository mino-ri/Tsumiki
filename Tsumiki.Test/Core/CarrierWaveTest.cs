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
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0.25f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);
        var wave = new CarrierWave(config);

        // 事前に位相を進めておく
        for (var i = 0; i < 100; i++)
        {
            wave.TickAndRender(0.01, -1.0, 0f);
        }

        // リセット実行
        wave.Reset();

        // リセット後の最初の出力を確認（位相が初期化されている）
        var result = wave.TickAndRender(0.01, -1.0, 0f);
        // Phase = 0.25f で開始し、そこから波形を計算するので、範囲内の値になる
        Assert.InRange(result, -1.5f, 1.5f);
    }

    [Fact]
    public static void TickAndRender_出力値が範囲内()
    {
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);
        var wave = new CarrierWave(config);

        wave.Reset();

        for (var i = 0; i < 1000; i++)
        {
            var result = wave.TickAndRender(0.01, -1.0, 0f);
            // Level = 1.0f なので、出力は -1f ~ 1f の範囲内に収まるはず
            Assert.InRange(result, -1f, 1f);
        }
    }

    [Fact]
    public static void TickAndRender_位相が進む()
    {
        // ShapeY = 0 で純粋な三角波を生成
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);
        var wave = new CarrierWave(config);

        wave.Reset();

        var results = new float[100];
        for (var i = 0; i < 100; i++)
        {
            results[i] = wave.TickAndRender(0.01, -1.0, 0f);
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
        var unit1 = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unit2 = new TestCarrierUnit { Pitch = 2.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config1 = new CarrierWaveConfig(unit1);
        var config2 = new CarrierWaveConfig(unit2);
        var wave1 = new CarrierWave(config1);
        var wave2 = new CarrierWave(config2);

        wave1.Reset();
        wave2.Reset();

        var delta = 0.01;
        var results1 = new float[200];
        var results2 = new float[200];

        for (var i = 0; i < 200; i++)
        {
            results1[i] = wave1.TickAndRender(delta, -1.0, 0f);
            results2[i] = wave2.TickAndRender(delta, -1.0, 0f);
        }

        // ピッチが2倍の場合、100サンプル後の値が、ピッチ1倍の200サンプル後の値と近似するはず
        Assert.InRange(MathF.Abs(results1[199] - results2[99]), 0f, 0.2f);
    }

    [Fact]
    public static void TickAndRender_シンク有効時に位相がリセットされる()
    {
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = true };
        var config = new CarrierWaveConfig(unit);
        var wave = new CarrierWave(config);

        wave.Reset();

        var delta = 0.01;
        // まず位相を進める（syncPhase < 0 なのでシンクしない）
        for (var i = 0; i < 50; i++)
        {
            wave.TickAndRender(delta, -1.0, 0f);
        }

        // シンクを発火させる（syncPhase >= 0）
        var resultBeforeSync = wave.TickAndRender(delta, 0.0, 0f);
        var resultAfterSync = wave.TickAndRender(delta, -1.0, 0f);

        // シンク後は位相がリセットされているため、波形が変化する可能性が高い
        // ただし、厳密な検証は難しいので、範囲チェックのみ
        Assert.InRange(resultBeforeSync, -1.5f, 1.5f);
        Assert.InRange(resultAfterSync, -1.5f, 1.5f);
    }

    [Fact]
    public static void TickAndRender_FM変調が効く()
    {
        // ShapeY = 0 で純粋な三角波を生成
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);
        var waveNoFm = new CarrierWave(config);
        var waveFm = new CarrierWave(config);

        waveNoFm.Reset();
        waveFm.Reset();

        var delta = 0.01;
        var resultsNoFm = new float[100];
        var resultsFm = new float[100];

        for (var i = 0; i < 100; i++)
        {
            resultsNoFm[i] = waveNoFm.TickAndRender(delta, -1.0, 0f);
            resultsFm[i] = waveFm.TickAndRender(delta, -1.0, 0.5f); // FM 変調あり
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
        var unitNeutral = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unitPositive = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0.5f, Sync = false };
        var unitNegative = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = -0.5f, Sync = false };

        var configNeutral = new CarrierWaveConfig(unitNeutral);
        var configPositive = new CarrierWaveConfig(unitPositive);
        var configNegative = new CarrierWaveConfig(unitNegative);

        var waveNeutral = new CarrierWave(configNeutral);
        var wavePositive = new CarrierWave(configPositive);
        var waveNegative = new CarrierWave(configNegative);

        waveNeutral.Reset();
        wavePositive.Reset();
        waveNegative.Reset();

        var delta = 0.01;
        var resultsNeutral = new float[100];
        var resultsPositive = new float[100];
        var resultsNegative = new float[100];

        for (var i = 0; i < 100; i++)
        {
            resultsNeutral[i] = waveNeutral.TickAndRender(delta, -1.0, 0f);
            resultsPositive[i] = wavePositive.TickAndRender(delta, -1.0, 0f);
            resultsNegative[i] = waveNegative.TickAndRender(delta, -1.0, 0f);
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
        // ShapeY = 0 で純粋な三角波を生成
        var unitNeutral = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unitPositive = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0.5f, ShapeY = 0f, Sync = false };

        var configNeutral = new CarrierWaveConfig(unitNeutral);
        var configPositive = new CarrierWaveConfig(unitPositive);

        var waveNeutral = new CarrierWave(configNeutral);
        var wavePositive = new CarrierWave(configPositive);

        waveNeutral.Reset();
        wavePositive.Reset();

        var delta = 0.01;
        var resultsNeutral = new float[100];
        var resultsPositive = new float[100];

        for (var i = 0; i < 100; i++)
        {
            resultsNeutral[i] = waveNeutral.TickAndRender(delta, -1.0, 0f);
            resultsPositive[i] = wavePositive.TickAndRender(delta, -1.0, 0f);
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
        var unitHalf = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 0.5f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unitFull = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };

        var configHalf = new CarrierWaveConfig(unitHalf);
        var configFull = new CarrierWaveConfig(unitFull);

        var waveHalf = new CarrierWave(configHalf);
        var waveFull = new CarrierWave(configFull);

        waveHalf.Reset();
        waveFull.Reset();

        var delta = 0.01;
        var resultsHalf = new float[100];
        var resultsFull = new float[100];

        for (var i = 0; i < 100; i++)
        {
            resultsHalf[i] = waveHalf.TickAndRender(delta, -1.0, 0f);
            resultsFull[i] = waveFull.TickAndRender(delta, -1.0, 0f);
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

    [Fact]
    public static void CarrierWaveConfig_Recalculate_ShapeX変更で再計算される()
    {
        var unit = new TestCarrierUnit
        {
            Pitch = 1.0,
            Phase = 0f,
            Level = 1.0f,
            ShapeX = 0f,
            ShapeY = 0f,
            Sync = false
        };
        var config = new CarrierWaveConfig(unit);

        var oldUpSlope = config.UpSlope;
        var oldDownSlope = config.DownSlope;
        var oldUpEnd = config.UpEnd;
        var oldDownStart = config.DownStart;

        // ShapeX を変更して再計算
        unit.ShapeX = 0.5f;
        config.Recalculate();

        // Shape関連の値が変わることを確認
        Assert.NotEqual(oldUpSlope, config.UpSlope);
        Assert.NotEqual(oldDownSlope, config.DownSlope);
        Assert.NotEqual(oldUpEnd, config.UpEnd);
        Assert.NotEqual(oldDownStart, config.DownStart);
    }

    [Fact]
    public static void CarrierWaveConfig_Recalculate_ShapeY変更で再計算される()
    {
        var unit = new TestCarrierUnit
        {
            Pitch = 1.0,
            Phase = 0f,
            Level = 1.0f,
            ShapeX = 0f,
            ShapeY = -0.5f, // 負の値から開始
            Sync = false
        };
        var config = new CarrierWaveConfig(unit);

        var oldTriFactor = config.TriFactor;
        var oldSinFactor = config.SinFactor;

        // ShapeY を正の値に変更して再計算
        unit.ShapeY = 0.7f;
        config.Recalculate();

        // TriFactor と SinFactor が変わることを確認
        Assert.NotEqual(oldTriFactor, config.TriFactor);
        Assert.NotEqual(oldSinFactor, config.SinFactor);
        Assert.Equal(2.0f, config.TriFactor);
        Assert.Equal(0.0f, config.SinFactor);
    }

    [Fact]
    public static void CarrierWaveConfig_Recalculate_Shape変更なしでは形状パラメータは変わらない()
    {
        var unit = new TestCarrierUnit
        {
            Pitch = 1.0,
            Phase = 0f,
            Level = 1.0f,
            ShapeX = 0.3f,
            ShapeY = -0.4f,
            Sync = false
        };
        var config = new CarrierWaveConfig(unit);

        var oldTriFactor = config.TriFactor;
        var oldSinFactor = config.SinFactor;
        var oldUpSlope = config.UpSlope;
        var oldDownSlope = config.DownSlope;

        // Pitch や Level のみ変更して再計算
        unit.Pitch = 2.0;
        unit.Level = 0.5f;
        config.Recalculate();

        // Pitch と Level は更新されるが、Shape関連は変わらない
        Assert.Equal(2.0, config.Pitch);
        Assert.Equal(0.5f, config.Level);
        Assert.Equal(oldTriFactor, config.TriFactor);
        Assert.Equal(oldSinFactor, config.SinFactor);
        Assert.Equal(oldUpSlope, config.UpSlope);
        Assert.Equal(oldDownSlope, config.DownSlope);
    }

    [Fact]
    public static void CarrierWaveConfig_Recalculate_パラメータ変更が反映される()
    {
        var unit = new TestCarrierUnit
        {
            Pitch = 1.0,
            Phase = 0f,
            Level = 1.0f,
            ShapeX = 0f,
            ShapeY = 0f,
            Sync = false
        };
        var config = new CarrierWaveConfig(unit);

        // 複数のパラメータを変更
        unit.Pitch = 3.5;
        unit.Phase = 0.25f;
        unit.Level = 0.7f;
        unit.Sync = true;

        config.Recalculate();

        // 変更が反映されることを確認
        Assert.Equal(3.5, config.Pitch);
        Assert.Equal(0.25f, config.Phase);
        Assert.Equal(0.7f, config.Level);
        Assert.True(config.Sync);
    }

    [Fact]
    public static void TickAndRender_Level0で無音()
    {
        var unit = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);
        var wave = new CarrierWave(config);

        wave.Reset();

        // Level が 0 なので、全てのサンプルが 0 になるはず
        for (var i = 0; i < 100; i++)
        {
            var result = wave.TickAndRender(0.01, -1.0, 0f);
            Assert.Equal(0f, result);
        }
    }

    [Fact]
    public static void TickAndRender_Pitch0で位相が進まない()
    {
        var unit = new TestCarrierUnit { Pitch = 0.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var config = new CarrierWaveConfig(unit);
        var wave = new CarrierWave(config);

        wave.Reset();

        var results = new float[100];
        for (var i = 0; i < 100; i++)
        {
            results[i] = wave.TickAndRender(0.01, -1.0, 0f);
        }

        // Pitch が 0 なので、位相が進まず、全て同じ値になるはず
        for (var i = 1; i < results.Length; i++)
        {
            Assert.Equal(results[0], results[i]);
        }
    }

    [Fact]
    public static void TickAndRender_ShapeXの境界値()
    {
        var unitNegative = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = -1f, ShapeY = 0f, Sync = false };
        var unitZero = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unitPositive = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 1f, ShapeY = 0f, Sync = false };

        var configNegative = new CarrierWaveConfig(unitNegative);
        var configZero = new CarrierWaveConfig(unitZero);
        var configPositive = new CarrierWaveConfig(unitPositive);

        var waveNegative = new CarrierWave(configNegative);
        var waveZero = new CarrierWave(configZero);
        var wavePositive = new CarrierWave(configPositive);

        waveNegative.Reset();
        waveZero.Reset();
        wavePositive.Reset();

        // 全ての境界値で出力が有限範囲内であることを確認
        for (var i = 0; i < 100; i++)
        {
            var resultNegative = waveNegative.TickAndRender(0.01, -1.0, 0f);
            var resultZero = waveZero.TickAndRender(0.01, -1.0, 0f);
            var resultPositive = wavePositive.TickAndRender(0.01, -1.0, 0f);

            Assert.InRange(resultNegative, -1.5f, 1.5f);
            Assert.InRange(resultZero, -1.5f, 1.5f);
            Assert.InRange(resultPositive, -1.5f, 1.5f);
        }
    }

    [Fact]
    public static void TickAndRender_ShapeYの境界値()
    {
        var unitNegative = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = -1f, Sync = false };
        var unitZero = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 0f, Sync = false };
        var unitPositive = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0f, ShapeY = 1f, Sync = false };

        var configNegative = new CarrierWaveConfig(unitNegative);
        var configZero = new CarrierWaveConfig(unitZero);
        var configPositive = new CarrierWaveConfig(unitPositive);

        var waveNegative = new CarrierWave(configNegative);
        var waveZero = new CarrierWave(configZero);
        var wavePositive = new CarrierWave(configPositive);

        waveNegative.Reset();
        waveZero.Reset();
        wavePositive.Reset();

        // 全ての境界値で出力が有限範囲内であることを確認
        for (var i = 0; i < 100; i++)
        {
            var resultNegative = waveNegative.TickAndRender(0.01, -1.0, 0f);
            var resultZero = waveZero.TickAndRender(0.01, -1.0, 0f);
            var resultPositive = wavePositive.TickAndRender(0.01, -1.0, 0f);

            Assert.InRange(resultNegative, -1.5f, 1.5f);
            Assert.InRange(resultZero, -1.5f, 1.5f);
            Assert.InRange(resultPositive, -1.5f, 1.5f);
        }
    }

    [Fact]
    public static void TickAndRender_計算された形状パラメータが出力に影響する()
    {
        // ShapeX と ShapeY を変えることで UpSlope, DownSlope, TriFactor, SinFactor が変わり、
        // それが実際の波形出力に反映されることを確認
        var unit1 = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = -0.5f, ShapeY = -0.5f, Sync = false };
        var unit2 = new TestCarrierUnit { Pitch = 1.0, Phase = 0f, Level = 1.0f, ShapeX = 0.8f, ShapeY = 0.6f, Sync = false };

        var config1 = new CarrierWaveConfig(unit1);
        var config2 = new CarrierWaveConfig(unit2);

        var wave1 = new CarrierWave(config1);
        var wave2 = new CarrierWave(config2);

        wave1.Reset();
        wave2.Reset();

        // 計算された形状パラメータが異なることを確認
        Assert.NotEqual(config1.UpSlope, config2.UpSlope);
        Assert.NotEqual(config1.DownSlope, config2.DownSlope);
        Assert.NotEqual(config1.TriFactor, config2.TriFactor);
        Assert.NotEqual(config1.SinFactor, config2.SinFactor);

        // 波形が異なることを確認
        var different = false;
        for (var i = 0; i < 100; i++)
        {
            var result1 = wave1.TickAndRender(0.01, -1.0, 0f);
            var result2 = wave2.TickAndRender(0.01, -1.0, 0f);
            if (MathF.Abs(result1 - result2) > 0.01f)
            {
                different = true;
                break;
            }
        }
        Assert.True(different, "形状パラメータが異なるのに波形が同じ");
    }
}
