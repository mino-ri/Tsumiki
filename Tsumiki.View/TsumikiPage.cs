namespace Tsumiki.View;

public partial class TsumikiPage() : PanelBase<TabPageControl>(new RectF(0f, 0f, 1f, 1f)), IControl
{
    private PointF _mouseDownPoint;
    private Control? _hovered;
    private Control? _mouseCaptured;
    private Control? _focused;
    private TabPageControl? _currentTab;

    public event Action<IVisual>? RenderRequested;

    public new void OnKeyDown(char key, VirtualKeyCode keyCode, KeyModifier modifiers)
    {
        _focused?.OnKeyDown(key, keyCode, modifiers);
    }

    public new void OnKeyUp(char key, VirtualKeyCode keyCode, KeyModifier modifiers)
    {
        _focused?.OnKeyUp(key, keyCode, modifiers);
    }

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
        if (_mouseCaptured != _focused)
        {
            _focused?.OnLostFocus();
            _focused = null;
        }

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
        (_mouseCaptured ?? _hovered)?.OnWheel(distance);
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        if (_currentTab is { })
        {
            return _currentTab.TryFindParameter(point, out parameterId);
        }

        parameterId = 0;
        return false;
    }

    internal override void SetTabPageType(TabPageType tabPageType)
    {
        var newTab = Children.FirstOrDefault(c => c.TabPageType == tabPageType);
        if (newTab is { } && _currentTab != newTab)
        {
            _currentTab?.Deactivate();
            newTab.Activate();
            _currentTab = newTab;
        }
    }

    internal override Control? FindControl(PointF point)
    {
        return _currentTab?.FindControl(point);
    }

    internal override void RenderCore(IDrawingContext context)
    {
        _currentTab?.RenderCore(context);
    }

    internal override void Focus(Control control)
    {
        if (_focused != control)
        {
            _focused?.OnLostFocus();
            _focused = control;
        }
    }

    internal override void Unfocus(Control control)
    {
        if (_focused == control)
        {
            _focused?.OnLostFocus();
            _focused = null;
        }
    }

    public bool TryFindParameter(float x, float y, out int parameterId) => TryFindParameter(new PointF(x, y), out parameterId);

    internal override void RequestRender(Control control) => RenderRequested?.Invoke(control);
}
