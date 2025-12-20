using System.Runtime.InteropServices;

namespace Tsumiki.View;

public interface ITsumikiCanvas : IDisposable
{
    /// <summary>描画範囲を変更します。</summary>
    void Resize(Rect rect);
}

public readonly record struct Rect(int Left, int Top, int Right, int Bottom)
{
    public readonly int Width => Right - Left;
    public readonly int Height => Bottom - Top;
}

public readonly record struct PointF(float X, float Y)
{
    public static PointF operator +(PointF a, PointF b)
        => new(a.X + b.X, a.Y + b.Y);

    public static PointF operator -(PointF a, PointF b)
        => new(a.X - b.X, a.Y - b.Y);

    public static PointF operator *(PointF a, float b)
        => new(a.X * b, a.Y * b);
}

public readonly record struct SizeF(float Width, float Height)
{
    public static SizeF operator +(SizeF a, SizeF b)
        => new(a.Width + b.Width, a.Height + b.Height);

    public static SizeF operator -(SizeF a, SizeF b)
        => new(a.Width - b.Width, a.Height - b.Height);

    public static SizeF operator *(SizeF a, float b)
        => new(a.Width * b, a.Height * b);
}

public readonly record struct RectF(float Left, float Top, float Right, float Bottom)
{
    public readonly float Width => Right - Left;
    public readonly float Height => Bottom - Top;
    public readonly PointF Location => new(Left, Top);
    public readonly SizeF Size => new(Width, Height);

    public bool Contains(PointF point)
        => Contains(point.X, point.Y);

    public bool Contains(float x, float y)
        => x >= Left && x <= Right && y >= Top && y <= Bottom;

    public static RectF operator +(RectF rect, PointF point)
        => new(rect.Left + point.X, rect.Top + point.Y, rect.Right + point.X, rect.Bottom + point.Y);

    public static RectF operator -(RectF rect, PointF point)
        => new(rect.Left - point.X, rect.Top - point.Y, rect.Right - point.X, rect.Bottom - point.Y);
}

public interface IControl : IVisual
{
    event Action<IVisual>? RenderRequested;

    void OnKeyDown(char key, VirtualKeyCode keyCode, KeyModifier modifiers);
    void OnKeyUp(char key, VirtualKeyCode keyCode, KeyModifier modifiers);
    void OnWheel(float distance);
    void OnMouseMove(float x, float y);
    void OnLeftButtonDown(float x, float y);
    void OnLeftButtonUp(float x, float y);
    void OnLeftButtonDoubleClick(float x, float y);
    void OnParameterChanged(int parameterId);
    bool TryFindParameter(float x, float y, out int parameterId);
}

public interface IVisual
{
    void Render(IDrawingContext context);
}

public interface IDrawingContext
{
    void Clear();
    void SetResourceImage(TabPageType tabPageType);
    void DrawImage(in RectF clientRange, in RectF imageRange);
    void DrawFilterGraph(in RectF clientRange, float normalizedCutoff, float resonance);
    void DrawCarrierGraph(in RectF clientRange, in GraphParameters parameters, in FmParameters fmParameters);
    void DrawModulatorGraph(in RectF clientRange, in GraphParameters parameters);
}

[StructLayout(LayoutKind.Sequential)]
public struct GraphParameters
{
    public float X;
    public float Y;
    public float Pitch;
    public float Period;
}

[StructLayout(LayoutKind.Sequential)]
public struct FmParameters
{
    public float X;
    public float Y;
    public float Pitch;
    public float Period;
    public float Level;
    private readonly float _p0;
    private readonly float _p1;
    private readonly float _p2;
}

public enum VirtualKeyCode : short
{
    Back = 1,
    Tab,
    Clear,
    Return,
    Pause,
    Escape,
    Space,
    Next,
    End,
    Home,
    Left,
    Up,
    Right,
    Down,
    PageUp,
    PageDown,
    Select,
    Print,
    Enter,
    Snapshot,
    Insert,
    Delete,
    Help,
    NumPad0,
    NumPad1,
    NumPad2,
    NumPad3,
    NumPad4,
    NumPad5,
    NumPad6,
    NumPad7,
    NumPad8,
    NumPad9,
    Multiply,
    Add,
    Separator,
    Subtract,
    Decimal,
    Divide,
    F1,
    F2,
    F3,
    F4,
    F5,
    F6,
    F7,
    F8,
    F9,
    F10,
    F11,
    F12,
    NumLock,
    Scroll,
    Shift,
    Control,
    Alt,
    Equals,
    ContextMenu,
    MediaPlay,
    MediaStop,
    MediaPrev,
    MediaNext,
    VolumeUp,
    VolumeDown,
    F13,
    F14,
    F15,
    F16,
    F17,
    F18,
    F19,
    F20,
    F21,
    F22,
    F23,
    F24,
    Super,
    FirstCode = Back,
    LastCode = Super,
    VKEY_FIRST_ASCII = 128,
}

[Flags]
public enum KeyModifier : short
{
    Shift = 1 << 0,
    Alternate = 1 << 1,
    Command = 1 << 2,
    Control = 1 << 3,
}

[Flags]
public enum TabPageType
{
    Main = 0,
    Modulation = 1,
    Tuning = 2,
}
