namespace Tsumiki.View;

internal class XYControl<TX, TY>(IRangeViewParameter<TX> xData, IRangeViewParameter<TY> yData, RectF control, RectF texture)
    : Control(control)
{
    private readonly SizeF _foregroundSize = TextureToControl(texture.Size);
    private readonly SizeF _motionSize = control.Size - TextureToControl(texture.Size);
    private readonly int _xStepCount = xData.StepCount == 0 ? 128 : xData.StepCount;
    private readonly int _yStepCount = yData.StepCount == 0 ? 128 : yData.StepCount;
    private RectF _controlRect;
    private double _lastX = -1.0;
    private double _lastY = -1.0;
    private bool _xDragging;
    private bool _yDragging;
    private double _dragStartX;
    private double _dragStartY;

    public IRangeViewParameter<TX> XData { get; } = xData;
    public IRangeViewParameter<TY> YData { get; } = yData;

    private void Recalculate()
    {
        var newX = XData.NormalizedValue;
        var newY = YData.NormalizedValue;
        if (newX == _lastX && newY == _lastY) return;
        _lastX = newX;
        _lastY = newY;
        var left = (float)(GlobalRect.Left + _motionSize.Width * _lastX);
        var top = (float)(GlobalRect.Top + _motionSize.Height * (1.0 - _lastY));
        _controlRect = new RectF(left, top, left + _foregroundSize.Width, top + _foregroundSize.Height);
    }

    internal override void OnLeftButtonDown(PointF point)
    {
        var range = _controlRect - GlobalRect.Location;
        _xDragging = range.Top <= point.Y && point.Y <= range.Bottom;
        _yDragging = range.Left <= point.X && point.X <= range.Right;
        _dragStartX = XData.NormalizedValue;
        _dragStartY = YData.NormalizedValue;

        if (_xDragging && !_yDragging)
        {
            XData.BeginEdit();
        }
        else if (!_xDragging && _yDragging)
        {
            YData.BeginEdit();
        }
    }

    internal override void OnLeftButtonUp(PointF point)
    {
        if (_xDragging && !_yDragging)
        {
            XData.EndEdit();
        }
        else if (!_xDragging && _yDragging)
        {
            YData.EndEdit();
        }

        _xDragging = false;
        _yDragging = false;
    }

    internal override void OnMouseDrag(PointF point, PointF mouseDownPoint)
    {
        var delta = point - mouseDownPoint;

        if (_xDragging)
        {
            var deltaX = delta.X / _motionSize.Width;
            var newX = Math.Round(Math.Clamp(_dragStartX + deltaX, 0.0, 1.0) * _xStepCount) / _xStepCount;
            if (XData.NormalizedValue != newX)
            {
                if (_xDragging && _yDragging)
                {
                    XData.BeginEdit();
                    XData.NormalizedValue = newX;
                    XData.EndEdit();
                }
                else
                {
                    XData.NormalizedValue = newX;
                }
            }
        }

        if (_yDragging)
        {
            var deltaY = delta.Y / -_motionSize.Height;
            var newY = Math.Round(Math.Clamp(_dragStartY + deltaY, 0.0, 1.0) * _yStepCount) / _yStepCount;
            if (YData.NormalizedValue != newY)
            {
                if (_xDragging && _yDragging)
                {
                    YData.BeginEdit();
                    YData.NormalizedValue = newY;
                    YData.EndEdit();
                }
                else
                {
                    YData.NormalizedValue = newY;
                }
            }
        }

        if (_xDragging || _yDragging)
        {
            RequestRender();
        }
    }

    internal override void OnLeftButtonDoubleClick(PointF point)
    {
        XData.BeginEdit();
        XData.NormalizedValue = XData.DefaultNormalizedValue;
        XData.EndEdit();

        YData.BeginEdit();
        YData.NormalizedValue = YData.DefaultNormalizedValue;
        YData.EndEdit();

        RequestRender();
    }

    internal override void RenderCore(IDrawingContext context)
    {
        Recalculate();
        context.DrawImage(in _controlRect, in texture);
    }

    public override void OnParameterChanged(int parameterId)
    {
        if (parameterId != XData.Id && parameterId != YData.Id) return;

        RequestRender();
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        var range = _controlRect - GlobalRect.Location;
        if (range.Left <= point.X && point.X <= range.Right)
        {
            parameterId = YData.Id;
            return true;
        }

        if (range.Top <= point.Y && point.Y <= range.Bottom)
        {
            parameterId = XData.Id;
            return true;
        }

        parameterId = default;
        return false;
    }
}
