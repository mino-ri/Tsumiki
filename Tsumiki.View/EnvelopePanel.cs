namespace Tsumiki.View;

internal class EnvelopePanel(
    IRangeViewParameter<int> attack,
    IRangeViewParameter<int> decay,
    IRangeViewParameter<float> sustain,
    IRangeViewParameter<int> release,
    RectF rect,
    RectF envelopControl,
    RectF attackImage,
    RectF decayImage,
    RectF underImage) : Panel(rect)
{
    private readonly RectF _envelopControl = envelopControl;
    private readonly float _envWidth = envelopControl.Width / 3f;

    internal override void RequestRender(Control control)
    {
        // エンベロープコントロールは互いに重なり合うため、常にパネル上の全コントロールを描画し直す
        base.RequestRender(this);
    }

    internal override void RenderCore(IDrawingContext context)
    {
        var envelopControl = _envelopControl + GlobalRect.Location;
        var releaseLeft = envelopControl.Left + envelopControl.Width / 1.5f;
        var attackRight = (float)(envelopControl.Left + _envWidth * attack.NormalizedValue);
        var decayRight = (float)(attackRight + _envWidth * decay.NormalizedValue);
        var releaseRight = (float)(releaseLeft + _envWidth * release.NormalizedValue);
        var decayCenter = (float)(envelopControl.Bottom - envelopControl.Height * sustain.NormalizedValue);

        context.DrawImage(envelopControl with { Right = attackRight }, in attackImage);
        context.DrawImage(new RectF(attackRight, envelopControl.Top, decayRight, decayCenter), in decayImage);
        context.DrawImage(new RectF(attackRight, decayCenter, releaseLeft, envelopControl.Bottom), in underImage);
        context.DrawImage(new RectF(releaseLeft, decayCenter, releaseRight, envelopControl.Bottom), in decayImage);

        base.RenderCore(context);
    }
}
