using Tsumiki;
using Tsumiki.View;
using Tsumiki.View.Win;

var rect = new Rect(0, 0, 960, 640);
using var form = new Form
{
    Text = "Tsumiki",
    ClientSize = new Size(rect.Width, rect.Height),
    MinimizeBox = false,
    MaximizeBox = false,
    BackColor = Color.FromArgb(80, 69, 48),
};
form.MinimumSize = new Size(form.Width, form.Height);
form.MaximumSize = new Size(form.Width + rect.Width, form.Height + rect.Height);

TsumikiPage? page = null;
ITsumikiCanvas? canvas = null;
try
{
    form.Resize += (_, _) =>
    {
        var clientSize = form.ClientSize;
        var newRect = new Rect(0, 0, clientSize.Width, clientSize.Height);
        canvas?.Resize(newRect);
    };

    form.Shown += (_, _) =>
    {
        var model = new TsumikiModel();
        page = TsumikiPage.Create(new TsumikiViewModel(model, null));
        model.ParameterValueChanged += parameter => page.OnParameterChanged(parameter.Id.Value);
        canvas = TsumikiCanvas.Create(form.Handle, rect, page);
    };

    form.KeyDown += (_, e) =>
    {
        if (e.KeyCode == Keys.Back)
        {
            page?.OnKeyDown(' ', VirtualKeyCode.Back, KeyModifier.None);
        }
    };

    form.KeyPress += (_, e) =>
    {
        page?.OnKeyDown(e.KeyChar, (VirtualKeyCode)e.KeyChar, KeyModifier.None);
    };

    Application.Run(form);
}
finally
{
    canvas?.Dispose();
}
