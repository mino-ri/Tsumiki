using IndirectX.Helper;

namespace Tsumiki.View.Win;

public sealed partial class WinTsumikiCanvas : ITsumikiCanvas
{
    private readonly nint _hwnd;
    private readonly Renderer _renderer;
    private readonly RenderLoop _renderLoop;

    public unsafe static WinTsumikiCanvas? Create(nint parentHandle, Rect rect)
    {
        WinInterop.RegisterClass();
        var hwnd = WinInterop.CreateWindow(parentHandle, rect);
        return hwnd == nint.Zero ? null : new WinTsumikiCanvas(hwnd, rect.Width, rect.Height);
    }

    private WinTsumikiCanvas(nint hwnd, int width, int height)
    {
        _hwnd = hwnd;
        _renderer = new Renderer(hwnd, width, height);
        _renderLoop = RenderLoop.Run(_renderer.Frame);
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
        WinInterop.ResizeWindow(_hwnd, normalized);
        _renderer.Resize(normalized.Width, normalized.Height);
    }

    private bool _isDisposed;
    private bool Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // マネージドリソースの解放
            }

            _renderLoop.Stop();
            _renderer.Dispose();
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

    ~WinTsumikiCanvas()
    {
        Dispose(false);
    }
}
