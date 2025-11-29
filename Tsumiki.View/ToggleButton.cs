namespace Tsumiki.View;

internal class ToggleButton(IViewParameter data, RectF control, RectF texture)
    : Control<IViewParameter>(data, control)
{
    private readonly RectF _mouseAvailableRect = control - control.Location;
    private readonly RectF _rect0 = texture;
    private readonly RectF _rect1 = texture + new PointF(texture.Width, 0f);

    internal override void OnLeftButtonUp(PointF point)
    {
        if (!_mouseAvailableRect.Contains(point)) return;

        Data.BeginEdit();
        Data.NormalizedValue = Data.NormalizedValue > 0.5 ? 0.0 : 1.0;
        Data.EndEdit();
        RequestRender();
    }

    internal override void RenderCore(IDrawingContext context)
    {
        if (Data.NormalizedValue > 0.5)
        {
            context.DrawImage(in GlobalRect, in _rect1);
        }
        else
        {
            context.DrawImage(in GlobalRect, in _rect0);
        }
    }

    public override void OnParameterChanged(int parameterId)
    {
        if (parameterId != Data.Id) return;

        RequestRender();
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        parameterId = Data.Id;
        return true;
    }
}
