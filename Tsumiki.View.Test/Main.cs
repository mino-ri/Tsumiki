using Tsumiki.View;
using Tsumiki.View.Win;

var rect = new Rect(0, 0, 960, 640);
using var form = new Form
{
    Text = "Tsumiki",
    ClientSize = new Size(rect.Width, rect.Height),
    MinimizeBox = false,
    MaximizeBox = false,
};
form.MinimumSize = new Size(form.Width, form.Height);

WinTsumikiCanvas? canvas = null;
form.Resize += (_, e) =>
{
    var clientRect = form.ClientRectangle;
    var newRect = new Rect(0, 0, clientRect.Width, clientRect.Height);
    canvas?.Resize(newRect);
};

form.Shown += (_, e) =>
{
    canvas = WinTsumikiCanvas.Create(form.Handle, rect);
};

Application.Run(form);
