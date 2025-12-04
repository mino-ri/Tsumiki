namespace Tsumiki.View;

internal class DelayPanel(
    IViewParameter delay,
    IViewParameter feedback,
    RectF rect,
    RectF delayControl,
    RectF image) : Panel(rect)
{
    private const float HalfWidth = 15f / PixelControlWidth;
    private readonly SizeF _barSize = new(30f / PixelControlWidth, delayControl.Height);

    internal override void RequestRender(Control control)
    {
        // コントロールが互いに重なり合うため、常にパネル上の全コントロールを描画し直す
        base.RequestRender(this);
    }

    internal override void RenderCore(IDrawingContext context)
    {
        var targetRect = delayControl + GlobalRect.Location;
        var right = targetRect.Left + targetRect.Width * 1.45f;
        var xStep = Math.Max((float)(delay.NormalizedValue * targetRect.Width), 2f / PixelControlWidth);
        var heightFactor = (float)feedback.NormalizedValue;
        var height = _barSize.Height;
        var imageHeight = image.Height;

        for (var x = targetRect.Left; x < right; x += xStep)
        {
            context.DrawImage(new RectF(
                x - HalfWidth,
                targetRect.Bottom - height,
                x + HalfWidth,
                targetRect.Bottom
                ), image with { Bottom = image.Top + imageHeight });
            height *= heightFactor;
            imageHeight *= heightFactor;
        }

        base.RenderCore(context);
    }
}
