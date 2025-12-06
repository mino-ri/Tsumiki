namespace Tsumiki.View;

internal class TabSwitcher(TabPageType tabPageType, RectF control, RectF texture) : Control(control)
{
    private readonly RectF _mouseAvailableRect = control - control.Location;

    internal override void OnLeftButtonUp(PointF point)
    {
        if (!_mouseAvailableRect.Contains(point)) return;
        Parent?.SetTabPageType(tabPageType);
    }

    public override void OnParameterChanged(int parameterId) { }

    internal override void RenderCore(IDrawingContext context)
    {
        context.DrawImage(in GlobalRect, in texture);
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        parameterId = 0;
        return false;
    }
}
