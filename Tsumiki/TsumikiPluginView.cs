using System;
using NPlug;

namespace Tsumiki;

public class TsumikiPluginView(TsumikiModel model) : IAudioPluginView
{
    private readonly TsumikiModel _model = model;
    private IAudioPluginFrame? _frame;

    public ViewRectangle Size
    {
        get
        {
            TsumikiLogger.WriteAccess([]);
            return field;
        }

        private set;
    } = new(0, 0, 400, 400);

    public bool IsPlatformTypeSupported(AudioPluginViewPlatform platform)
    {
        TsumikiLogger.WriteAccess([(int)platform]);
        return platform == AudioPluginViewPlatform.Hwnd;
    }

    public void Attached(nint parent, AudioPluginViewPlatform type)
    {
        TsumikiLogger.WriteAccess([parent, (int)type]);
    }

    public bool CanResize()
    {
        TsumikiLogger.WriteAccess([]);
        return true;
    }

    public bool CheckSizeConstraint(ref ViewRectangle rect)
    {
        TsumikiLogger.WriteAccess([rect.Left, rect.Right, rect.Top, rect.Bottom]);
        return true;
    }

    public void OnSize(ViewRectangle newSize)
    {
        TsumikiLogger.WriteAccess([newSize.Left, newSize.Right, newSize.Top, newSize.Bottom]);
        Size = newSize;
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
