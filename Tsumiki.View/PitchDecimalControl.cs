namespace Tsumiki.View;

internal class PitchDecimalControl<T>(IRangeViewParameter<T> data, RectF control, RectF texture, int stepCount, int integerStepCount, int digitCount)
    : DragControl<T>(data, control, texture)
{
    private readonly RectF _originalTexture = texture;

    protected override void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect)
    {
        var baseSteps = (int)Math.Round(value * stepCount);
        var steps = baseSteps / (stepCount / digitCount) % 10;
        textureRect = new RectF(
            _originalTexture.Left + _originalTexture.Width * steps,
            _originalTexture.Top,
            _originalTexture.Left + _originalTexture.Width * (steps + 1),
            _originalTexture.Bottom);
    }

    protected override double GetDragDelta(PointF pointDelta) => pointDelta.Y * -64.0 / digitCount;

    internal override void OnLeftButtonDoubleClick(PointF point)
    {
        Data.BeginEdit();
        Data.NormalizedValue = Math.Truncate(Data.NormalizedValue * integerStepCount) / integerStepCount;
        Data.EndEdit();
        RequestRender();
    }

    internal override void OnMouseDrag(PointF point, PointF mouseDownPoint)
    {
        var delta = GetDragDelta(point - mouseDownPoint);
        var newValue = Math.Round(Math.Clamp(_dragStartValue + Math.Round(delta * digitCount) / digitCount, 0.0, 1.0) * stepCount) / stepCount;

        Data.NormalizedValue = newValue;
        RequestRender();
    }
}
