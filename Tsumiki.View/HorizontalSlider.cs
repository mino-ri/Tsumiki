namespace Tsumiki.View;

internal class HorizontalSlider<T> : DragControl<T>
{
    private readonly SizeF _foregroundSize;
    private readonly float _motionWidth;

    public HorizontalSlider(IRangeViewParameter<T> data, RectF control, RectF texture)
         : base(data, control, texture)
    {
        _foregroundSize = TextureToControl(texture.Size);
        _motionWidth = control.Width - _foregroundSize.Width;
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

    protected override double GetDragDelta(PointF pointDelta) => pointDelta.X  * 4.0;
}
