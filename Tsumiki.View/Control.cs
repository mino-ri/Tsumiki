using System.Runtime.CompilerServices;

namespace Tsumiki.View;

public abstract class Control(RectF rect) : IVisual
{
    private readonly RectF _rect = rect;
    private RectF _globalRect = rect;
    private RectF _textureRect = ControlToTexture(in rect);

    public Panel? Parent { get; private set; }
    /// <summary>親コントロール内での位置。</summary>
    public ref readonly RectF Rect => ref _rect;

    /// <summary>描画領域全体の中での位置</summary>
    public ref readonly RectF GlobalRect => ref _globalRect;

    internal bool CanCaptureKey { get; init; } = false;

    internal virtual void OnKeyDown(char key, VirtualKeyCode keyCode, KeyModifier modifiers) { }

    internal virtual void OnKeyUp(char key, VirtualKeyCode keyCode, KeyModifier modifiers) { }

    internal virtual void OnLeftButtonDoubleClick(PointF point) { }

    internal virtual void OnLeftButtonDown(PointF point) { }

    internal virtual void OnLeftButtonUp(PointF point) { }

    internal virtual void OnMouseDrag(PointF point, PointF mouseDownPoint) { }

    internal virtual void OnWheel(float distance) { }

    internal void RequestRender() => Parent?.RequestRender(this);

    public abstract void OnParameterChanged(int parameterId);

    internal abstract bool TryFindParameter(PointF point, out int parameterId);

    public void Render(IDrawingContext context)
    {
        context.DrawImage(in _globalRect, in _textureRect);
        RenderCore(context);
    }

    internal void SetParent(Panel parent)
    {
        _globalRect = Rect + parent.GlobalRect.Location;
        _textureRect = ControlToTexture(in _globalRect);
        Parent = parent;
    }

    internal virtual Control FindControl(PointF point) => this;

    internal abstract void RenderCore(IDrawingContext context);

    protected static RectF ControlToTexture(in RectF controlRect)
    {
        // 背景は描画範囲とアスペクト比が違い、右側25%にコントロール群が配置されている
        return new(controlRect.Left * 0.75f, controlRect.Top, controlRect.Right * 0.75f, controlRect.Bottom);
    }

    protected static SizeF TextureToControl(in SizeF textureSize)
    {
        return new(textureSize.Width / 0.75f, textureSize.Height);
    }

    const float PixelControlWidth = 1920f;
    const float PixelControlHeight = 1280f;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static RectF PixelToControl(int x, int y, int width, int height)
    {
        return new RectF(x / PixelControlWidth, y / PixelControlHeight, (x + width) / PixelControlWidth, (y + height) / PixelControlHeight);
    }

    const float PixelTextureWidth = 2560f;
    const float PixelTextureHeight = 1280f;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static RectF PixelToTexture(int x, int y, int width, int height)
    {
        return new RectF(x / PixelTextureWidth, y / PixelTextureHeight, (x + width) / PixelTextureWidth, (y + height) / PixelTextureHeight);
    }
}

public abstract class Control<TData>(TData data, RectF rect) : Control(rect)
{
    public TData Data { get; } = data;
}
