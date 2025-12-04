using System.Collections;

namespace Tsumiki.View;

public class Panel(RectF rect) : Control(rect), IEnumerable<Control>
{
    private readonly List<Control> _controls = [];

    private Control[] Children => field ??= [.. _controls];

    internal virtual void RequestRender(Control control) => Parent?.RequestRender(control);

    internal override Control FindControl(PointF point)
    {
        for (var i = Children.Length - 1; i >= 0; i--)
        {
            var control = Children[i];
            if (control.Rect.Contains(point))
            {
                return control.FindControl(point - control.Rect.Location);
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

    internal override void SetParent(Panel parent)
    {
        base.SetParent(parent);
        foreach (var control in Children)
        {
            control.SetParent(this);
        }
    }

    internal void Add(Control control)
    {
        control.SetParent(this);
        _controls.Add(control);
    }

    IEnumerator<Control> IEnumerable<Control>.GetEnumerator() => ((IEnumerable<Control>)_controls).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_controls).GetEnumerator();
}
