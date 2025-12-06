namespace Tsumiki.View;

public class TabPageControl(TabPageType tabPageType) : Panel(new RectF(0f, 0f, 1f, 1f))
{
    public TabPageType TabPageType { get; } = tabPageType;

    public bool IsActive { get; private set; }

    public override void Render(IDrawingContext context)
    {
        if (!IsActive) return;

        context.SetResourceImage(TabPageType);
        base.Render(context);
    }

    internal override void RequestRender(Control control)
    {
        if (!IsActive) return;

        base.RequestRender(control);
    }

    internal void Activate()
    {
        IsActive = true;
        RequestRender();
    }

    internal void Deactivate()
    {
        IsActive = false;
    }
}
