using IndirectX.Helper;

namespace Tsumiki.View.Win;

public sealed partial class TsumikiCanvas : ITsumikiCanvas
{
    private readonly nint _hwnd;
    private readonly Renderer _renderer;
    private readonly RenderLoop _renderLoop;
    private readonly IControl _control;
    private int _width;
    private int _height;

    public unsafe static ITsumikiCanvas? Create(nint parentHandle, Rect rect, IControl control)
    {
        WinInterop.RegisterClass();
        var hwnd = WinInterop.CreateWindow(parentHandle, rect);
        return hwnd == nint.Zero ? null : new TsumikiCanvas(hwnd, rect.Width, rect.Height, control);
    }

    private TsumikiCanvas(nint hwnd, int width, int height, IControl page)
    {
        _hwnd = hwnd;
        _renderer = new Renderer(hwnd, width, height, page);
        _renderLoop = RenderLoop.Run(_renderer.Frame);
        _width = width;
        _height = height;
        _control = page;
        _control.RenderRequested += _renderer.RegisterVisual;
        WinInterop.AddCanvas(hwnd, this);
    }

    public void Resize(Rect rect)
    {
        var normalizedByWidth = new Rect(rect.Left, rect.Top,
            rect.Left + rect.Width / 3 * 3, rect.Top + rect.Width / 3 * 2);
        var normalizedByHeight = new Rect(rect.Left, rect.Top,
            rect.Left + rect.Height / 2 * 3, rect.Top + rect.Height / 2 * 2);
        var normalized = normalizedByWidth.Width < normalizedByHeight.Width
            ? normalizedByWidth
            : normalizedByHeight;
        _width = normalized.Width;
        _height = normalized.Height;
        WinInterop.ResizeWindow(_hwnd, normalized);
        _renderer.Resize(normalized.Width, normalized.Height);
    }

    internal void OnMouseMove(short x, short y)
    {
        _control.OnMouseMove((float)x / _width, (float)y / _height);
    }

    internal void OnLeftButtonDown(short x, short y)
    {
        _control.OnLeftButtonDown((float)x / _width, (float)y / _height);
    }

    internal void OnLeftButtonUp(short x, short y)
    {
        _control.OnLeftButtonUp((float)x / _width, (float)y / _height);
    }

    internal void OnLeftButtonDoubleClick(short x, short y)
    {
        _control.OnLeftButtonDoubleClick((float)x / _width, (float)y / _height);
    }

    private bool _isDisposed;
    private bool Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // マネージドリソースの解放
                _control.RenderRequested -= _renderer.RegisterVisual;
            }

            _renderLoop.Stop();
            _renderer.Dispose();
            WinInterop.RemoveCanvas(_hwnd);
            WinInterop.DisposeWindow(_hwnd);

            _isDisposed = true;
        }

        return _isDisposed;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~TsumikiCanvas()
    {
        Dispose(false);
    }
}
