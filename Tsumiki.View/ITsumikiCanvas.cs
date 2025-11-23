namespace Tsumiki.View;

public interface ITsumikiCanvas : IDisposable
{
    void Resize(Rect rect);
}

public readonly record struct Rect(int Left, int Top, int Right, int Bottom)
{
    public readonly int Width => Right - Left;
    public readonly int Height => Bottom - Top;
}
