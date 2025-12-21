using System.Collections;

namespace Tsumiki.View;

public abstract class PanelBase(RectF rect) : Control(rect)
{
    internal virtual void RequestRender(Control control) => Parent?.RequestRender(control);

    internal virtual void SetTabPageType(TabPageType tabPageType) => Parent?.SetTabPageType(tabPageType);

    internal virtual void Focus(Control control) => Parent?.Focus(control);

    internal virtual void Unfocus(Control control) => Parent?.Unfocus(control);
}

public class PanelBase<T>(RectF rect) : PanelBase(rect), IEnumerable<T>
    where T : Control
{
    private readonly List<T> _controls = [];

    public T[] Children => field ??= [.. _controls];


    internal override Control? FindControl(PointF point)
    {
        for (var i = Children.Length - 1; i >= 0; i--)
        {
            var control = Children[i];
            if (control.Rect.Contains(point) && control.FindControl(point - control.Rect.Location) is { } target)
            {
                return target;
            }
        }

        return this;
    }

    public override void OnParameterChanged(int parameterId)
    {
        foreach (var control in Children)
        {
            control.OnParameterChanged(parameterId);
        }
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        for (var i = Children.Length - 1; i >= 0; i--)
        {
            var control = Children[i];
            if (control.Rect.Contains(point))
            {
                return control.TryFindParameter(point - control.Rect.Location, out parameterId);
            }
        }

        parameterId = default;
        return false;
    }

    internal override void RenderCore(IDrawingContext context)
    {
        foreach (var control in Children)
        {
            control.RenderCore(context);
        }
    }

    internal override void SetParent(PanelBase parent)
    {
        base.SetParent(parent);
        foreach (var control in Children)
        {
            control.SetParent(this);
        }
    }

    internal void Add(T control)
    {
        control.SetParent(this);
        _controls.Add(control);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_controls).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_controls).GetEnumerator();
}

public class Panel(RectF rect) : PanelBase<Control>(rect);
