namespace Tsumiki.View;

internal class PitchDecimalControl<T>(IRangeViewParameter<T> data, RectF control, RectF texture, int stepCount, int digitCount)
    : DragControl<T>(data, control, texture)
{
    private readonly RectF _originalTexture = texture;

    protected override void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect)
    {
        var steps = (int)(value * digitCount) % 10;
        textureRect = new RectF(
            _originalTexture.Left + _originalTexture.Width * steps,
            _originalTexture.Top,
            _originalTexture.Left + _originalTexture.Width * (steps + 1),
            _originalTexture.Bottom);
    }

    protected override double GetDragDelta(PointF pointDelta) => pointDelta.Y * -4.0 / 1000.0;

    internal override void OnLeftButtonDoubleClick(PointF point)
    {
        Data.BeginEdit();
        Data.NormalizedValue = Math.Round(Data.NormalizedValue * 16.0) / 16.0;
        Data.EndEdit();
        RequestRender();
    }

    internal override void OnMouseDrag(PointF point, PointF mouseDownPoint)
    {
        var delta = GetDragDelta(point - mouseDownPoint);
        var newValue = Math.Round(Math.Clamp(_dragStartValue + delta, 0.0, 1.0) * stepCount) / stepCount;

        Data.NormalizedValue = newValue;
        RequestRender();
    }
}
