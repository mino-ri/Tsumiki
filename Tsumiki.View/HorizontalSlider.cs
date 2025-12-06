namespace Tsumiki.View;

internal class HorizontalSlider<T> : DragControl<T>
{
    private readonly SizeF _foregroundSize;
    private readonly float _motionWidth;
    private readonly double _xDragFactor;

    public HorizontalSlider(IRangeViewParameter<T> data, RectF control, RectF texture, bool isHitAllArea = true, double xDragFactor = 6.0)
         : base(data, control, texture, isHitAllArea)
    {
        _foregroundSize = TextureToControl(texture.Size);
        _motionWidth = control.Width - _foregroundSize.Width;
        _xDragFactor = xDragFactor;
    }

    protected override void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect)
    {
        var foregroundLeft = (float)(value * _motionWidth);
        controlRect = new RectF(
            GlobalRect.Left + foregroundLeft,
            GlobalRect.Top,
            GlobalRect.Left + _foregroundSize.Width + foregroundLeft,
            GlobalRect.Top + _foregroundSize.Height);
    }

    protected override double GetDragDelta(PointF pointDelta) => pointDelta.X * _xDragFactor;
}
