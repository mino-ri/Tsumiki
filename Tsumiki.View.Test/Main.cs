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
        canvas = TsumikiCanvas.Create(form.Handle, rect, TsumikiPage.Create(new TsumikiViewModel(new TsumikiModel())));
    };

    Application.Run(form);
}
finally
{
    canvas?.Dispose();
}
