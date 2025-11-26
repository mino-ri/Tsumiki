using System.Numerics;
using System.Runtime.InteropServices;

namespace Tsumiki.View.Win;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
    public Vector4 Vector;
    public Vector2 Texture;

    public Vertex(Vector4 vector, Vector2 texture)
    {
        Vector = vector;
        Texture = texture;
    }

    public Vertex(float x, float y, float z, float w, Vector2 texture)
    {
        Vector = new Vector4(x, y, z, w);
        Texture = texture;
    }

    public override readonly string ToString()
    {
        return $"({Vector.X:F3}, {Vector.Y:F3}, {Vector.Z:F3}, {Vector.W:F3})";
    }
}
