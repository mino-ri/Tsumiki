using Tsumiki.Metadata;

namespace Tsumiki.Core;

internal static class MathT
{
    /// <summary>0～1で表される位相から、三角波を返します。</summary>
    [AudioTiming]
    public static float Tri(float x)
    {
        var a = x - 0.75f;
        return MathF.Abs((a - MathF.Round(a)) * 4f) - 1f;
    }


    /// <summary>-0.5～0.5の範囲で表される三角波から、sin関数に近似する値を返します。</summary>
    [AudioTiming]
    public static float TriToSin(float tri)
    {
        var tri2 = tri * tri;
        return (((tri2 * -0.540347f + 2.53566f) * tri2 - 5.16651f) * tri2 + 3.14159f) * tri;
    }

    /// <summary>0～1 で表される位相から、sin関数に近似する値を返します。</summary>
    [AudioTiming]
    public static float Sin(float x)
    {
        var a = x - 0.75f;
        var tri = MathF.Abs((a - MathF.Round(a)) * 2f) - 0.5f;
        return TriToSin(tri);
    }

    [AudioTiming]
    public static float Saw(float x) => (x >= 0.5f ? x - 1f : x) * 2f;

    [AudioTiming]
    public static float Sqr(float x) => x >= 0.5 ? 1f : -1f;
}
