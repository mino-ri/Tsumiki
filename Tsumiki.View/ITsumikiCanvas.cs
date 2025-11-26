namespace Tsumiki.View;

public interface ITsumikiCanvas : IDisposable
{
    /// <summary>描画範囲を変更します。</summary>
    void Resize(Rect rect);
    /// <summary>次の描画タイミングで Visual を描画します。</summary>
    void DrawVisual(IVisual visual);
}

public readonly record struct Rect(int Left, int Top, int Right, int Bottom)
{
    public readonly int Width => Right - Left;
    public readonly int Height => Bottom - Top;
}

public readonly record struct RectF(float Left, float Top, float Right, float Bottom)
{
    public readonly float Width => Right - Left;
    public readonly float Height => Bottom - Top;
}

public interface IControl : IVisual
{
    void OnKeyDown(ushort key, short keyCode, short modifiers);
    void OnKeyUp(ushort key, short keyCode, short modifiers);
    void OnWheel(float distance);
    void OnMouseMove(float x, float y);
    void OnLeftButtonDown(float x, float y);
    void OnLeftButtonUp(float x, float y);
    void OnLeftButtonDoubleClick(float x, float y);
}

public interface IVisual
{
    void Render(IDrawingContext context);
}

public interface IDrawingContext
{
    void Clear();
    void DrawImage(in RectF clientRange, in RectF imageRange);
}
