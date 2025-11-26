namespace Tsumiki.View;

public abstract class Control<TData>(TData data, RectF rect) : IControl
{
    // 背景は描画範囲とアスペクト比が違い、右側25%にコントロール群が配置されている
    private readonly RectF _backRect = new(rect.Left * 0.75f, rect.Top, rect.Right * 0.75f, rect.Bottom);
    private readonly RectF _rect = rect;
    public TData Data { get; } = data;
    public RectF Rect => _rect;

    public virtual void OnKeyDown(ushort key, short keyCode, short modifiers) { }

    public virtual void OnKeyUp(ushort key, short keyCode, short modifiers) { }

    public virtual void OnLeftButtonDoubleClick(float x, float y) { }

    public virtual void OnLeftButtonDown(float x, float y) { }

    public virtual void OnLeftButtonUp(float x, float y) { }

    public virtual void OnMouseMove(float x, float y) { }

    public virtual void OnWheel(float distance) { }

    public void Render(IDrawingContext context)
    {
        context.DrawImage(in _rect, in _backRect);
        RenderCore(context);
    }

    public abstract void RenderCore(IDrawingContext context);
}
