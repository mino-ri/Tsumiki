namespace Tsumiki.View;

internal partial class TuningValueControl
{
    private IRangeViewParameter<int>? _draggingParameter;
    private double _dragStartValue;

    internal override void OnLeftButtonDown(PointF point)
    {
    }

    internal override void OnMouseDrag(PointF point, PointF mouseDownPoint)
    {
        var dragDelta = (int)((point.Y - mouseDownPoint.Y) * -160f);
        if (dragDelta != 0 && _draggingParameter is null)
        {
            _draggingParameter = mouseDownPoint.X < _halfSize.Width
                ? mouseDownPoint.Y < _halfSize.Height ? n : d
                : mouseDownPoint.Y < _halfSize.Height ? pn : pd;
            _draggingParameter.BeginEdit();
            _dragStartValue = _draggingParameter.NormalizedValue;
        }

        if (_draggingParameter is { })
        {
            var range = _draggingParameter.MaxValue - _draggingParameter.MinValue;
            var newValue = Math.Clamp((Math.Round(_dragStartValue * range) + dragDelta) / range, 0.0, 1.0);
            _draggingParameter.NormalizedValue = newValue;
        }
    }
}
