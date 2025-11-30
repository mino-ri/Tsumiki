namespace Tsumiki.View;

internal abstract class DragControl<T>(IRangeViewParameter<T> data, RectF control, RectF texture)
    : Control<IRangeViewParameter<T>>(data, control)
{
    private readonly int _stepCount = data.StepCount == 0 ? 128 : data.StepCount;
    private double _dragStartValue;
    private double _lastValue = -1f;
    private RectF _textureRect = texture;
    private RectF _controlRect = control;

    protected abstract void RecalculateRect(double value, ref RectF controlRect, ref RectF textureRect);

    protected abstract double GetDragDelta(PointF pointDelta);

    private void Recalculate()
    {
        var newValue = Data.NormalizedValue;
        if (newValue == _lastValue) return;
        _lastValue = newValue;

        _controlRect = GlobalRect;
        RecalculateRect(_lastValue, ref _controlRect, ref _textureRect);
    }

    internal override void OnLeftButtonDown(PointF point)
    {
        _dragStartValue = Data.NormalizedValue;
        Data.BeginEdit();
    }

    internal override void OnLeftButtonUp(PointF point)
    {
        Data.EndEdit();
    }

    internal override void OnMouseDrag(PointF point, PointF mouseDownPoint)
    {
        var delta = GetDragDelta(point - mouseDownPoint);
        var newValue = Math.Round(Math.Clamp(_dragStartValue + delta, 0.0, 1.0) * _stepCount) / _stepCount;

        Data.NormalizedValue = newValue;
        RequestRender();
    }

    internal override void OnLeftButtonDoubleClick(PointF point)
    {
        Data.BeginEdit();
        Data.NormalizedValue = Data.DefaultNormalizedValue;
        Data.EndEdit();
        RequestRender();
    }

    internal override void RenderCore(IDrawingContext context)
    {
        Recalculate();
        context.DrawImage(in _controlRect, in _textureRect);
    }

    public override void OnParameterChanged(int parameterId)
    {
        if (parameterId != Data.Id) return;

        RequestRender();
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        parameterId = Data.Id;
        return true;
    }
}
