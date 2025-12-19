using System;
using NPlug;
using Tsumiki.View;
using Tsumiki.View.Win;

namespace Tsumiki;

public class TsumikiPluginView(TsumikiViewModel viewModel) : IAudioPluginView
{
    private TsumikiPage? _page;
    private ITsumikiCanvas? _canvas;
    private const int MinWidth = 960;
    private const int MinHeight = 640;
    private const int MaxWidth = 1920;
    private const int MaxHeight = 1280;

    public ViewRectangle Size { get; private set; } = new(0, 0, MinWidth, MinHeight);

    public bool IsPlatformTypeSupported(AudioPluginViewPlatform platform)
    {
        TsumikiLogger.WriteAccess([(int)platform]);
        return platform == AudioPluginViewPlatform.Hwnd;
    }

    public void Attached(nint parent, AudioPluginViewPlatform type)
    {
        TsumikiLogger.WriteAccess([parent, (int)type]);
        try
        {
            _canvas = TsumikiCanvas.Create(parent, ToTsumikiViewSize(Size), _page ??= TsumikiPage.Create(viewModel));
        }
        catch (Exception ex)
        {
            TsumikiLogger.WriteException(ex);
        }
    }

    public bool CanResize()
    {
        TsumikiLogger.WriteAccess([]);
        return true;
    }

    public bool CheckSizeConstraint(ref ViewRectangle rect)
    {
        TsumikiLogger.WriteAccess([rect.Left, rect.Right, rect.Top, rect.Bottom]);
        var requestedWidth = rect.Right - rect.Left;
        var requestedHeight = rect.Bottom - rect.Top;
        var adjustedWidth = Math.Clamp(requestedWidth, MinWidth, MaxWidth);
        var adjustedHeight = Math.Clamp(requestedHeight, MinHeight, MaxHeight);
        // ウィンドウを小さくしようとしているか
        var currentSize = Size;
        var currentWidth = currentSize.Right - currentSize.Left;
        var currentHeight = currentSize.Bottom - currentSize.Top;
        var isSmaller = currentWidth <= adjustedWidth && currentHeight <= adjustedHeight;
        var baseSize = isSmaller
            ? Math.Min(adjustedWidth / 3, adjustedHeight / 2)
            : Math.Max((int)Math.Ceiling(adjustedWidth / 3.0), (int)Math.Ceiling(adjustedHeight / 2.0));
        var actualWidth = baseSize * 3;
        var actualHeight = baseSize * 2;

        if (actualWidth != requestedWidth || actualHeight != requestedHeight)
        {
            rect = new ViewRectangle(rect.Left, rect.Top, rect.Left + actualWidth, rect.Top + actualHeight);
            return false;
        }

        return true;
    }

    public void OnSize(ViewRectangle newSize)
    {
        try
        {
            TsumikiLogger.WriteAccess([newSize.Left, newSize.Right, newSize.Top, newSize.Bottom]);
            var baseSize = Math.Min((newSize.Right - newSize.Left) / 3, (newSize.Bottom - newSize.Top) / 2);
            var actualNewSize = new ViewRectangle(newSize.Left, newSize.Top, newSize.Left + baseSize * 3, newSize.Top + baseSize * 2);
            Size = actualNewSize;
            _canvas?.Resize(ToTsumikiViewSize(actualNewSize));
        }
        catch (Exception ex)
        {
            TsumikiLogger.WriteException(ex);
        }
    }

    public void OnFocus(bool state)
    {
        TsumikiLogger.WriteAccess([state]);
    }

    public void OnKeyDown(ushort key, short keyCode, short modifiers)
    {
        TsumikiLogger.WriteAccess([key, keyCode, modifiers]);
        _page?.OnKeyDown((char)key, (VirtualKeyCode)keyCode, (KeyModifier)modifiers);
    }

    public void OnKeyUp(ushort key, short keyCode, short modifiers)
    {
        TsumikiLogger.WriteAccess([key, keyCode, modifiers]);
        _page?.OnKeyUp((char)key, (VirtualKeyCode)keyCode, (KeyModifier)modifiers);
    }

    public void OnWheel(float distance)
    {
        TsumikiLogger.WriteAccess([distance]);
        _page?.OnWheel(distance);
    }

    public void Removed()
    {
        TsumikiLogger.WriteAccess([]);
        try
        {
            _canvas?.Dispose();
            _canvas = null;
            _page = null;
        }
        catch (Exception ex)
        {
            TsumikiLogger.WriteException(ex);
        }
    }

    public void SetContentScaleFactor(float factor)
    {
        TsumikiLogger.WriteAccess([factor]);
    }

    public void SetFrame(IAudioPluginFrame frame)
    {
        TsumikiLogger.WriteAccess([]);
    }

    public void OnParameterChanged(AudioParameterId parameterId)
    {
        _page?.OnParameterChanged(parameterId.Value);
    }

    public bool TryFindParameter(int xPos, int yPos, out AudioParameterId parameterId)
    {
        TsumikiLogger.WriteAccess([xPos, yPos]);
        if (_page is not null && _page.TryFindParameter(xPos, yPos, out var id))
        {
            parameterId = id;
            return true;
        }

        parameterId = default;
        return false;
    }

    private static Rect ToTsumikiViewSize(ViewRectangle rect) => new(rect.Left, rect.Top, rect.Right, rect.Bottom);
}
