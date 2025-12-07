namespace Tsumiki.View;

internal class GlideSlider : DragControl<int>
{
    private readonly SizeF _foregroundSize;
    private readonly float _motionHeight;
    private readonly double _yDragFactor;

    public GlideSlider(IRangeViewParameter<int> data, RectF control, RectF texture, bool isHitAllArea = true, double yDragFactor = -4.0)
         : base(data, control, texture, isHitAllArea)
    {
        _foregroundSize = TextureToControl(texture.Size);
        _motionHeight = control.Height - _foregroundSize.Height;
        _yDragFactor = yDragFactor;
    }

    protected override void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect)
    {
        var glide = (int)Math.Round(value * 101.0) - 1;
        var viewValue = glide < 0 ? 0 : glide + 28;
        var foregroundTop = (float)((1.0 - viewValue / 128.0) * _motionHeight);
        controlRect = new RectF(
            GlobalRect.Left,
            GlobalRect.Top + foregroundTop,
            GlobalRect.Left + _foregroundSize.Width,
            GlobalRect.Top + _foregroundSize.Height + foregroundTop);
    }

    protected override double GetDragDelta(PointF pointDelta) => pointDelta.Y * _yDragFactor;
}
