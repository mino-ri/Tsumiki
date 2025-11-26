using System;
using NPlug;
using Tsumiki.View;
using Tsumiki.View.Win;

namespace Tsumiki;

public class TsumikiPluginView(TsumikiModel model) : IAudioPluginView
{
    private readonly TsumikiPage _page = new(new TsumikiViewModel(model));
    private IAudioPluginFrame? _frame;
    private ITsumikiCanvas? _canvas;
    private const int MinWidth = 960;
    private const int MinHeight = 640;
    private const int MaxWidth = 1920;
    private const int MaxHeight = 1280;

    public ViewRectangle Size
    {
        get
        {
            TsumikiLogger.WriteAccess([]);
            return field;
        }

        private set;
    } = new(0, 0, MinWidth, MinHeight);

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
            _canvas = TsumikiCanvas.Create(parent, ToTsumikiViewSize(Size), _page);
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
            Size = newSize;
            _canvas?.Resize(ToTsumikiViewSize(newSize));
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
        _page.OnKeyDown(key, keyCode, modifiers);
    }

    public void OnKeyUp(ushort key, short keyCode, short modifiers)
    {
        TsumikiLogger.WriteAccess([key, keyCode, modifiers]);
        _page.OnKeyUp(key, keyCode, modifiers);
    }

    public void OnWheel(float distance)
    {
        TsumikiLogger.WriteAccess([distance]);
        _page.OnWheel(distance);
    }

    public void Removed()
    {
        TsumikiLogger.WriteAccess([]);
        try
        {
            _canvas?.Dispose();
            _canvas = null;
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
        _frame = frame;
    }

    public bool TryFindParameter(int xPos, int yPos, out AudioParameterId parameterId)
    {
        TsumikiLogger.WriteAccess([xPos, yPos]);
        parameterId = default;
        return false;
    }

    private static Rect ToTsumikiViewSize(ViewRectangle rect) => new(rect.Left, rect.Top, rect.Right, rect.Bottom);
}
