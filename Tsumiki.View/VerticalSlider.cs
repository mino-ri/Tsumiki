namespace Tsumiki.View;

internal class VerticalSlider<T> : DragControl<T>
{
    private readonly SizeF _foregroundSize;
    private readonly float _motionHeight;

    public VerticalSlider(IRangeViewParameter<T> data, RectF control, RectF texture)
         : base(data, control, texture)
    {
        _foregroundSize = TextureToControl(texture.Size);
        _motionHeight = control.Height - _foregroundSize.Height;
    }

    protected override void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect)
    {
        var foregroundTop = (float)((1.0 - value) * _motionHeight);
        controlRect = new RectF(
            GlobalRect.Left,
            GlobalRect.Top + foregroundTop,
            GlobalRect.Left + _foregroundSize.Width,
            GlobalRect.Top + _foregroundSize.Height + foregroundTop);
    }

    protected override double GetDragDelta(PointF pointDelta) => pointDelta.Y * -4.0;
}
