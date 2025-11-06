using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public static class MathTTest
{
    private static readonly float[] TestFloats = [0f, 0.125f, 0.25f, 0.375f, 0.5f, 0.625f, 0.75f, 0.875f];

    [Fact]
    public static void Tri()
    {
        foreach (var f in TestFloats)
        {
            Assert.InRange(MathT.Tri(f), -1f, 1f);
        }
    }

    [Fact]
    public static void Sin()
    {
        foreach (var f in TestFloats)
        {
            Assert.InRange(MathT.Tri(f), -1.001f, 1.001f);
        }
    }

    [Fact]
    public static void Saw()
    {
        foreach (var f in TestFloats)
        {
            Assert.InRange(MathT.Tri(f), -1.001f, 1.001f);
        }
    }

    [Fact]
    public static void Sqr()
    {
        foreach (var f in TestFloats)
        {
            Assert.InRange(MathT.Tri(f), -1f, 1f);
        }
    }
}
