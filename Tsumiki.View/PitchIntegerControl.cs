namespace Tsumiki.View;

internal class PitchIntegerControl<T>(IRangeViewParameter<T> data, RectF control, RectF texture, int yCount, int offset, int stepCount)
    : DragControl<T>(data, control, texture)
{
    private readonly int _stepCount = stepCount;
    private readonly RectF _originalTexture = texture;

    protected override void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect)
    {
        var steps = (int)(value * _stepCount) + offset;
        var x = steps / yCount;
        var y = steps % yCount;

        textureRect = new RectF(
            _originalTexture.Left + _originalTexture.Width * x,
            _originalTexture.Top + _originalTexture.Height * y,
            _originalTexture.Left + _originalTexture.Width * (x + 1),
            _originalTexture.Top + _originalTexture.Height * (y + 1));
    }

    protected override double GetDragDelta(PointF pointDelta) => pointDelta.Y * -4.0;

    internal override void OnMouseDrag(PointF point, PointF mouseDownPoint)
    {
        var delta = GetDragDelta(point - mouseDownPoint);
        Data.NormalizedValue = Math.Clamp(_dragStartValue + Math.Round(delta * _stepCount) / _stepCount, 0.0, 1.0);
        RequestRender();
    }
}
