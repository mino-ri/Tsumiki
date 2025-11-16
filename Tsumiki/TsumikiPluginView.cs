using System;
using NPlug;
using Tsumiki.View;
using Tsumiki.View.Win;

namespace Tsumiki;

public class TsumikiPluginView(TsumikiModel model) : IAudioPluginView
{
    private readonly TsumikiModel _model = model;
    private IAudioPluginFrame? _frame;
    private ITsumikiCanvas? _canvas;
    private const int MinWidth = 564;
    private const int MinHeight = 120;
    private const int MaxWidth = 1920;
    private const int MaxHeight = 1080;

    public ViewRectangle Size
    {
        get
        {
            TsumikiLogger.WriteAccess([]);
            return field;
        }

        private set;
    } = new(0, 0, 564, 120);

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
            _canvas = WinTsumikiCanvas.Create(parent, ToTsumikiViewSize(Size));
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

        if (adjustedWidth != requestedWidth || adjustedHeight != requestedHeight)
        {
            rect = new ViewRectangle(rect.Left, rect.Top, rect.Left + adjustedWidth, rect.Top + adjustedHeight);
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
    }

    public void OnKeyUp(ushort key, short keyCode, short modifiers)
    {
        TsumikiLogger.WriteAccess([key, keyCode, modifiers]);
    }

    public void OnWheel(float distance)
    {
        TsumikiLogger.WriteAccess([distance]);
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

    private static Rect ToTsumikiViewSize(ViewRectangle rect) => new(rect.Left, rect.Right, rect.Top, rect.Bottom);
}
