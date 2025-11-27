using System.Collections;

namespace Tsumiki.View;

public class Panel<TData>(TData data, RectF rect) : Control<TData>(data, rect), IEnumerable<Control>
{
    private readonly List<Control> _controls = [];

    private Control[] Children => field ??= [.. _controls];

    internal override Control FindControl(PointF point)
    {
        foreach (var control in Children)
        {
            if (control.Rect.Contains(point))
            {
                return control.FindControl(point + control.Rect.Location);
            }
        }

        return this;
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        foreach (var control in Children)
        {
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

    internal void Add(Control control)
    {
        control.SetParent(this);
        _controls.Add(control);
    }

    IEnumerator<Control> IEnumerable<Control>.GetEnumerator() => ((IEnumerable<Control>)_controls).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_controls).GetEnumerator();
}
