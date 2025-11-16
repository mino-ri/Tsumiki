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
        WinInterop.ResizeWindow(_hwnd, rect);
        _renderer.Resize(rect.Width, rect.Height);
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
