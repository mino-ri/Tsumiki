using Tsumiki.View;
using Tsumiki.View.Win;

var rect = new Rect(0, 800, 0, 600);
using var form = new Form
{
    Text = "Tsumiki",
    Width = rect.Width,
    Height = rect.Height,
    MinimumSize = new Size(400, 300),
    MinimizeBox = false,
    MaximizeBox = false,
};

WinTsumikiCanvas? canvas = null;
form.Resize += (_, e) =>
{
    var clientRect = form.ClientRectangle;
    var newRect = new Rect(0, clientRect.Width, 0, clientRect.Height);
    canvas?.Resize(newRect);
};

form.Shown += (_, e) =>
{
    canvas = WinTsumikiCanvas.Create(form.Handle, rect);
};

Application.Run(form);
