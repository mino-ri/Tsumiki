using System.Runtime.CompilerServices;
using Tsumiki.Metadata;

namespace Tsumiki.Core;

internal static class MathT
{
    public const int MaxVoices = 8;
    public const int MaxStackCount = 7;

    /// <summary>0～1で表される位相から、三角波を返します。</summary>
    [AudioTiming]
    public static float Tri(float x)
    {
        var a = x - 0.75f;
        return MathF.Abs((a - MathF.Round(a)) * 4f) - 1f;
    }

    /*
     次のような式で、パラメータ b を使って三角波から滑らかに正弦波に変化するような関数が作れる。
     f(x) = (1 + (π/2 - 1)b)x + b{ -π³/(2³ * 3!)x³ + π⁵/(2⁵ * 5!)x⁵ - π⁷/(2⁷ * 7!)x⁷ }
     もとの式における係数を a₁, a₃, a₅, a₇ とすると、
     f(x) = x + b(a₁x - a₃x³ + a₅x⁵ - a₇x⁷ )
     f(x) = x{ 1 + b(a₁ - a₃x² + a₅x⁴ - a₇x⁶) }
     f(x) = x{ 1 + b(a₁ + x²(-a₃ + a₅x² - a₇x⁴) }
     f(x) = x{ 1 + b(a₁ + x²(-a₃ + x²(a₅ - a₇x²)) }
     次数の逆順に並べて、
     f(x) = ((((- a₇x² + a₅) * x² - a₃) * x² + a₁) * b + 1) * x
     */

    private static readonly float A1 = (float)(Math.PI / 2.0);
    private static readonly float A1b = (float)(Math.PI / 2.0 - 1.0);
    private static readonly float A3 = (float)(Math.Pow(Math.PI, 3.0) / (8.0 * 6.0));
    private static readonly float A5 = (float)(Math.Pow(Math.PI, 5.0) / (32.0 * 120.0));
    private static readonly float A7 = (float)(Math.Pow(Math.PI, 7.0) / (128.0 * 5040.0));

    [AudioTiming]
    public static float TriToSin2(float tri, float b)
    {
        if (b <= 0f) return tri;
        var tri2 = tri * tri;
        return (b * (((-A7 * tri2 + A5) * tri2 - A3) * tri2 + A1b) + 1f) * tri;
    }

    /// <summary>-1～1の範囲で表される三角波から、sin関数に近似する値を返します。</summary>
    [AudioTiming]
    public static float TriToSin(float tri)
    {
        var tri2 = tri * tri;
        return (((-A7 * tri2 + A5) * tri2 - A3) * tri2 + A1) * tri;
    }

    /// <summary>0～1 で表される位相から、sin関数に近似する値を返します。</summary>
    [AudioTiming]
    public static float Sin(float x)
    {
        return TriToSin(Tri(x));
    }

    [AudioTiming]
    public static float Saw(float x) => (x >= 0.5f ? x - 1f : x) * 2f;

    [AudioTiming]
    public static float Sqr(float x) => x >= 0.5 ? 1f : -1f;

    [AudioTiming]
    public static double PitchToFreq(double pitch)
    {
        return 55.0 * Math.Pow(2.0, (Math.Min(127.0, pitch) - 33.0) / 12.0);
    }

    [AudioTiming]
    public static double PitchToDelta(double pitch, double sampleRate)
    {
        var frequency = 55.0 * Math.Pow(2.0, (Math.Min(127.0, pitch) - 33.0) / 12.0);
        return frequency / sampleRate;
    }

    public const double ExpThreshold = 1.0 / 16.0;

    /// <summary>
    /// 線形に変化する、エンベロープの1サンプルあたりの変化レートを返します。
    /// </summary>
    [AudioTiming]
    public static double GetEnvAttackDelta(int value, double sampleRate)
    {
        var seconds = Math.Pow(10, value / 20.0 - 3.0);
        return 1.0 / seconds / sampleRate;
    }

    /// <summary>
    /// 指数曲線で変化する、エンベロープの1サンプルあたりの変化レートを返します。
    /// </summary>
    [AudioTiming]
    public static double GetEnvelopeRate(int value, double sampleRate)
    {
        var seconds = Math.Pow(10, value / 20.0 - 3.0);
        return 1.0 - Math.Pow(ExpThreshold, 1.0 / (seconds * sampleRate));
    }

    public const double GlideExpThreshold = 1.0 / 32.0;

    /// <summary>
    /// 指数曲線で変化する、グライドの1サンプルあたりの変化レートを返します。
    /// </summary>
    [AudioTiming]
    public static double GetGlideRate(int seconds, double sampleRate)
    {
        return seconds <= 0
            ? 1.0
            : 1.0 - Math.Pow(GlideExpThreshold, 100.0 / (seconds * sampleRate));
    }

    /// <summary>パン(-1.0～1.0)の値から、左チャンネルの音量を取得します。合計値は常に2です。</summary>
    [AudioTiming]
    public static (float, float) GetPanLevel(float pan)
    {
        var sin = Sin(pan * 0.25f);
        return (1f - sin, 1f + sin);
    }
}
