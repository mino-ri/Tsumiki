namespace Tsumiki.View;

public partial class TsumikiPage() : Panel(new RectF(0f, 0f, 1f, 1f)), IControl
{
    private PointF _mouseDownPoint;
    private Control? _hovered;
    private Control? _mouseCaptured;

    public event Action<IVisual>? RenderRequested;

    public new void OnKeyDown(char key, VirtualKeyCode keyCode, KeyModifier modifiers) { }

    public new void OnKeyUp(char key, VirtualKeyCode keyCode, KeyModifier modifiers) { }

    public void OnLeftButtonDoubleClick(float x, float y)
    {
        var point = new PointF(x, y);
        var control = FindControl(point);
        control?.OnLeftButtonDoubleClick(point - control.GlobalRect.Location);
    }

    public void OnLeftButtonDown(float x, float y)
    {
        var point = new PointF(x, y);
        _mouseDownPoint = point;
        _mouseCaptured = FindControl(point);
        _mouseCaptured?.OnLeftButtonDown(point - _mouseCaptured.GlobalRect.Location);
    }

    public void OnLeftButtonUp(float x, float y)
    {
        var point = new PointF(x, y);
        _mouseCaptured?.OnLeftButtonUp(point - _mouseCaptured.GlobalRect.Location);
        _mouseCaptured = null;
    }

    public void OnMouseMove(float x, float y)
    {
        var point = new PointF(x, y);
        _hovered = FindControl(point);
        _mouseCaptured?.OnMouseDrag(point - _mouseCaptured.GlobalRect.Location, _mouseDownPoint - _mouseCaptured.GlobalRect.Location);
    }

    public new void OnWheel(float distance)
    {
        _hovered?.OnWheel(distance);
    }

    public bool TryFindParameter(float x, float y, out int parameterId) => TryFindParameter(new PointF(x, y), out parameterId);

    internal override void RequestRender(Control control) => RenderRequested?.Invoke(control);
}
