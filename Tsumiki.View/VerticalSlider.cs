namespace Tsumiki.View;

internal class VerticalSlider<T> : DragControl<T>
{
    private readonly SizeF _foregroundSize;
    private readonly float _motionHeight;
    private readonly float _motionLeft;
    private readonly double _yDragFactor;

    public VerticalSlider(IRangeViewParameter<T> data, RectF control, RectF texture, bool isHitAllArea = true, double yDragFactor = -4.0)
         : base(data, control, texture, isHitAllArea)
    {
        _foregroundSize = TextureToControl(texture.Size);
        _motionHeight = control.Height - _foregroundSize.Height;
        _motionLeft = (control.Width - _foregroundSize.Width) / 2f;
        _yDragFactor = yDragFactor;
    }

    protected override void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect)
    {
        var foregroundTop = (float)((1.0 - value) * _motionHeight);
        controlRect = new RectF(
            GlobalRect.Left + _motionLeft,
            GlobalRect.Top + foregroundTop,
            GlobalRect.Left + _motionLeft + _foregroundSize.Width,
            GlobalRect.Top + _foregroundSize.Height + foregroundTop);
    }

    protected override double GetDragDelta(PointF pointDelta) => pointDelta.Y * _yDragFactor;
}
