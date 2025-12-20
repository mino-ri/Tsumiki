namespace Tsumiki.View;

internal class DigitControl(IRangeViewParameter<int> data, RectF control, RectF texture, int step, int baseNumber = 10)
    : DragControl<int>(data, control, texture)
{
    private readonly RectF _originalTexture = texture;
    private readonly int _minimum = data.MinValue;
    private readonly int _range = data.MaxValue - data.MinValue;

    // 使わない
    protected override double GetDragDelta(PointF pointDelta) => pointDelta.Y * -160;

    protected override void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect)
    {
        var intValue = (int)Math.Round(value * _range) + _minimum;
        var divided = intValue / step;
        var steps = divided == 0 ? baseNumber : divided % baseNumber;
        textureRect = new RectF(
            _originalTexture.Left + _originalTexture.Width * steps,
            _originalTexture.Top,
            _originalTexture.Left + _originalTexture.Width * (steps + 1),
            _originalTexture.Bottom);
    }

    internal override void OnMouseDrag(PointF point, PointF mouseDownPoint)
    {
        var delta = (int)GetDragDelta(point - mouseDownPoint);
        var newValue = Math.Clamp((Math.Round(_dragStartValue * _range) + delta * step) / _range, 0.0, 1.0);
        Data.NormalizedValue = newValue;
        RequestRender();
    }
}
