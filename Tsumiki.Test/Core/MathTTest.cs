using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class MathTTest
{
    private static readonly float[] TestFloats = [0f, 0.125f, 0.25f, 0.375f, 0.5f, 0.625f, 0.75f, 0.875f];

    [Fact]
    public static void Tri_範囲チェック()
    {
        foreach (var f in TestFloats)
        {
            Assert.InRange(MathT.Tri(f), -1f, 1f);
        }
    }

    [Fact]
    public static void Tri_境界値チェック()
    {
        // MathT.Tri の実装を確認: x - 0.75 をベースに計算している
        // 位相0では 0 - 0.75 = -0.75, Round(-0.75) = -1, abs((-0.75 - (-1)) * 4) - 1 = abs(0.25 * 4) - 1 = 0
        Assert.Equal(0f, MathT.Tri(0f), 5);
        // 位相0.25では 0.25 - 0.75 = -0.5, Round(-0.5) = 0, abs((-0.5 - 0) * 4) - 1 = abs(-2) - 1 = 1
        Assert.Equal(1f, MathT.Tri(0.25f), 5);
        // 位相0.5では 0.5 - 0.75 = -0.25, Round(-0.25) = 0, abs((-0.25 - 0) * 4) - 1 = abs(-1) - 1 = 0
        Assert.Equal(0f, MathT.Tri(0.5f), 5);
        // 位相0.75では 0.75 - 0.75 = 0, Round(0) = 0, abs((0 - 0) * 4) - 1 = -1
        Assert.Equal(-1f, MathT.Tri(0.75f), 5);
    }

    [Fact]
    public static void TriToSin_範囲チェック()
    {
        // -0.5～0.5の範囲で三角波を入力
        for (var i = -5; i <= 5; i++)
        {
            var tri = i * 0.1f;
            var result = MathT.TriToSin(tri);
            Assert.InRange(result, -1.1f, 1.1f);
        }
    }

    [Fact]
    public static void TriToSin_sin関数との近似チェック()
    {
        // 三角波から生成したsin波と、実際のsin関数の値が近似しているか確認
        // MathT.Sin の内部実装と同様に、位相から三角波を生成してから sin に近似する
        for (var i = 0; i < 8; i++)
        {
            var phase = i / 8f;
            // MathT.Sin の内部実装: a = x - 0.75, tri = abs((a - round(a)) * 2) - 0.5
            var a = phase - 0.75f;
            var tri = MathF.Abs((a - MathF.Round(a)) * 2f) - 0.5f;
            var approx = MathT.TriToSin(tri);
            var expected = MathF.Sin(phase * 2f * MathF.PI);
            Assert.InRange(MathF.Abs(approx - expected), 0f, 0.02f);
        }
    }

    [Fact]
    public static void Sin_範囲チェック()
    {
        foreach (var f in TestFloats)
        {
            Assert.InRange(MathT.Sin(f), -1.001f, 1.001f);
        }
    }

    [Fact]
    public static void Sin_sin関数との近似チェック()
    {
        foreach (var f in TestFloats)
        {
            var expected = MathF.Sin(f * 2f * MathF.PI);
            var actual = MathT.Sin(f);
            Assert.InRange(MathF.Abs(actual - expected), 0f, 0.01f);
        }
    }

    [Fact]
    public static void Saw_範囲チェック()
    {
        foreach (var f in TestFloats)
        {
            Assert.InRange(MathT.Saw(f), -1.001f, 1.001f);
        }
    }

    [Fact]
    public static void Saw_境界値チェック()
    {
        // 位相0では0
        Assert.Equal(0f, MathT.Saw(0f), 5);
        // 位相0.25では0.5
        Assert.Equal(0.5f, MathT.Saw(0.25f), 5);
        // 位相0.5では-1
        Assert.Equal(-1f, MathT.Saw(0.5f), 5);
        // 位相0.75では-0.5
        Assert.Equal(-0.5f, MathT.Saw(0.75f), 5);
    }

    [Fact]
    public static void Sqr_範囲チェック()
    {
        foreach (var f in TestFloats)
        {
            Assert.InRange(MathT.Sqr(f), -1f, 1f);
        }
    }

    [Fact]
    public static void Sqr_境界値チェック()
    {
        // 位相0～0.5未満では-1
        Assert.Equal(-1f, MathT.Sqr(0f));
        Assert.Equal(-1f, MathT.Sqr(0.25f));
        Assert.Equal(-1f, MathT.Sqr(0.49f));
        // 位相0.5以上では1
        Assert.Equal(1f, MathT.Sqr(0.5f));
        Assert.Equal(1f, MathT.Sqr(0.75f));
        Assert.Equal(1f, MathT.Sqr(0.99f));
    }

    [Fact]
    public static void PitchToDelta_A4が正しい周波数になる()
    {
        var sampleRate = 44100.0;
        var pitch = 69.0; // A4
        var delta = MathT.PitchToDelta(pitch, sampleRate);
        var frequency = delta * sampleRate;
        // A4 = 440Hz
        Assert.Equal(440.0, frequency, 5);
    }

    [Fact]
    public static void PitchToDelta_オクターブで周波数が2倍になる()
    {
        var sampleRate = 44100.0;
        var delta1 = MathT.PitchToDelta(60.0, sampleRate); // C4
        var delta2 = MathT.PitchToDelta(72.0, sampleRate); // C5
        // オクターブ上がると周波数は2倍
        Assert.Equal(delta1 * 2.0, delta2, 5);
    }

    [Fact]
    public static void GetEnvelopeRate_正の値を返す()
    {
        var sampleRate = 44100.0;
        for (var i = 0; i <= 80; i += 10)
        {
            var rate = MathT.GetEnvelopeRate(i, sampleRate);
            Assert.InRange(rate, 0.0, 1.0);
        }
    }

    [Fact]
    public static void GetEnvelopeRate_値が大きいほどレートが小さい()
    {
        var sampleRate = 44100.0;
        var rate1 = MathT.GetEnvelopeRate(20, sampleRate);
        var rate2 = MathT.GetEnvelopeRate(40, sampleRate);
        var rate3 = MathT.GetEnvelopeRate(60, sampleRate);
        // GetEnvelopeRate の実装: 1 - pow(threshold, 1 / (seconds * sampleRate))
        // seconds = pow(10, value / 20 - 3) なので、value が大きいほど seconds も大きい
        // seconds が大きいほど、pow の指数部が小さくなり、結果的に rate が小さくなる
        // つまり、値が大きいほどレートは小さくなる（ゆっくり変化する）
        Assert.True(rate1 > rate2);
        Assert.True(rate2 > rate3);
    }
}
