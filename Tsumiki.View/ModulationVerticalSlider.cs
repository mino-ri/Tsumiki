namespace Tsumiki.View;

internal class ModulationVerticalSlider(IRangeViewParameter<double> data, RectF control, RectF backgroundTexture, RectF texture, bool isHitAllArea = true, double yDragFactor = -4.0)
    : VerticalSlider<double>(data, control, texture, isHitAllArea, yDragFactor)
{
    internal override void RenderCore(IDrawingContext context)
    {
        if (Data.Value == 0.0) return;

        context.DrawImage(in GlobalRect, in backgroundTexture);
        base.RenderCore(context);
    }
}
