using System;
using NPlug;
using Tsumiki.View;

namespace Tsumiki;

public class TsumikiPluginView(TsumikiModel model) : IAudioPluginView
{
    private readonly TsumikiModel _model = model;
    private ControlRoot? _viewRoot;
    private IAudioPluginFrame? _frame;

    private ViewRectangle _size = new(0, 0, 800, 450);
    public ViewRectangle Size
    {
        get
        {
            TsumikiLogger.WriteAccess([]);
            return _size;
        } 
    }

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
            _viewRoot = ViewInitializer.AttackInParentWindow(parent);
        }
        catch (Exception ex)
        {
            TsumikiLogger.WriteLog(ex.ToString());
        }
    }

    public bool CanResize()
    {
        TsumikiLogger.WriteAccess([]);
        return false;
    }

    public bool CheckSizeConstraint(ref ViewRectangle rect)
    {
        TsumikiLogger.WriteAccess([rect.Left, rect.Right, rect.Top, rect.Bottom]);
        return true;
    }

    public void OnSize(ViewRectangle newSize)
    {
        TsumikiLogger.WriteAccess([newSize.Left, newSize.Right, newSize.Top, newSize.Bottom]);

        _size = newSize;
        if (_viewRoot is null) return;
        _viewRoot.Width = newSize.Right - newSize.Left;
        _viewRoot.Height = newSize.Bottom - newSize.Top;
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
        _viewRoot?.Dispose();
        _viewRoot = null;
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
}
