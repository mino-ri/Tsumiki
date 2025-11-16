namespace Tsumiki.View;

public interface ITsumikiCanvas : IDisposable
{
    void Resize(Rect rect);
}

public readonly record struct Rect(int Left, int Right, int Top, int Bottom)
{
    public readonly int Width => Right - Left;
    public readonly int Height => Bottom - Top;
}
