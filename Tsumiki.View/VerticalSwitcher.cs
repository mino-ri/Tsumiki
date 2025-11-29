namespace Tsumiki.View;

internal class VerticalSwitcher<T>(IRangeViewParameter<T> data, RectF control, RectF texture, int yCount, int offset)
    : DragControl<T>(data, control, texture)
{
    private readonly RectF _originalTexture = texture;
    private readonly int _stepCount = data.StepCount == 0 ? 128 : data.StepCount;

    protected override void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect)
    {
        var steps = (int)Math.Round(value * _stepCount) + offset;
        var x = steps / yCount;
        var y = steps % yCount;

        textureRect = new RectF(
            _originalTexture.Left + _originalTexture.Width * x,
            _originalTexture.Top + _originalTexture.Height * y,
            _originalTexture.Left + _originalTexture.Width * (x + 1),
            _originalTexture.Top + _originalTexture.Height * (y + 1));
    }

    protected override double GetDragDelta(PointF pointDelta) => pointDelta.Y * -4.0;
}
