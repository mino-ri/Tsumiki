using System.Numerics;
using System.Runtime.InteropServices;
using IndirectX;

namespace Tsumiki.View.Win;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
    public Vector4 Vector;
    public Color Color;
    public Vector2 Texture;

    public Vertex(Vector4 vector, Color color, Vector2 texture)
    {
        Vector = vector;
        Color = color;
        Texture = texture;
    }

    public Vertex(float x, float y, float z, float w, Color color, Vector2 texture)
    {
        Vector = new Vector4(x, y, z, w);
        Color = color;
        Texture = texture;
    }

    public override readonly string ToString()
    {
        return $"({Vector.X:F3}, {Vector.Y:F3}, {Vector.Z:F3}, {Vector.W:F3})#{(int)(Color.A * 255f):x2}{(int)(Color.R * 255f):x2}{(int)(Color.G * 255f):x2}{(int)(Color.B * 255f):x2}";
    }
}
