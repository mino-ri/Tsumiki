using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tsumiki.View.Win;

internal static unsafe partial class WinInterop
{
    private const string ClassName = "TsumikiPlugin";
    private static bool _classRegistered = false;
    private static readonly Dictionary<nint, TsumikiCanvas> ActiveCanvases = [];

    public static void AddCanvas(nint hwnd, TsumikiCanvas canvas)
    {
        ActiveCanvases[hwnd] = canvas;
    }

    public static void RemoveCanvas(nint hwnd)
    {
        ActiveCanvases.Remove(hwnd);
    }

    public static void RegisterClass()
    {
        if (_classRegistered) return;

        var instance = GetModuleHandleW(null);
        if (instance == 0) return;

        using var className = new InteropString(ClassName);
        RegisterClassExW(new WindowClassEx
        {
            Size = (uint)Marshal.SizeOf<WindowClassEx>(),
            Style = CsVRedraw | CsHRedraw | CsDoubleClick,
            WndProc = &WndProc,
            Instance = instance,
            Cursor = LoadImageW(0, "#32512", 2, 0, 0, 0x0040 | 0x8000),
            ClassName = className.NativePtr,
        });

        _classRegistered = true;
    }

    const uint CsVRedraw = 0x0001;
    const uint CsHRedraw = 0x0002;
    const uint CsDoubleClick = 0x0008;

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint GetModuleHandleW(string? lpModuleName);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial ushort RegisterClassExW(in WindowClassEx wcx);

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint LoadImageW(nint instance, string name, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    public static nint CreateWindow(nint parentHandle, Rect rect)
    {
        return CreateWindowExW(0, ClassName, "", WsChild | WsVisible, rect.Left, rect.Top, rect.Width, rect.Height, parentHandle, nint.Zero, nint.Zero, 0);
    }

    const uint WsChild = 0x40000000;
    const uint WsVisible = 0x10000000;

    [LibraryImport("user32.dll")]
    private static partial nint CreateWindowExW(uint dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string pszClassName, [MarshalAs(UnmanagedType.LPWStr)] string pszWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nuint pParam);

    public static void ResizeWindow(nint hwnd, Rect rect)
    {
        SetWindowPos(hwnd, 0, rect.Left, rect.Top, rect.Width, rect.Height, SWP_NOZORDER | SWP_NOACTIVATE);
    }

    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_NOACTIVATE = 0x0010;

    [LibraryImport("user32.dll")]
    private static partial nint SetWindowPos(nint hwnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    public static void DisposeWindow(nint hwnd)
    {
        DestroyWindow(hwnd);
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(nint hwnd);

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct WindowClassEx
    {
        public uint Size;
        public uint Style;
        public delegate* unmanaged[Stdcall]<nint, WindowMessage, nint, nint, nint> WndProc;
        public int ClsExtra;
        public int WndExtra;
        public nint Instance;
        public nint Icon;
        public nint Cursor;
        public nint Background;
        public nint MenuName;
        public nint ClassName;
        public nint IconSm;
    }

    private readonly ref struct InteropString(string str)
    {
        public readonly nint NativePtr = Marshal.StringToHGlobalUni(str);
        public void Dispose() => Marshal.FreeHGlobal(NativePtr);
    }

    [LibraryImport("user32.dll")]
    private static partial nint DefWindowProcW(nint hWnd, WindowMessage uMsg, nint wParam, nint lParam);

    private static (short x, short y) GetCoordinates(nint lParam)
    {
        var x = (short)(ushort)((uint)lParam & 0xffffu);
        var y = (short)(ushort)(((uint)lParam >> 16) & 0xffffu);
        return (x, y);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static nint WndProc(nint hWnd, WindowMessage message, nint wParam, nint lParam)
    {
        TsumikiCanvas? canvas;
        switch (message)
        {
            // 子ウィンドウでは、Destroy に呼応して PostQuitMessage を送ってはいけない

            case WindowMessage.MouseMove:
                if (ActiveCanvases.TryGetValue(hWnd, out canvas))
                {
                    var (x, y) = GetCoordinates(lParam);
                    canvas.OnMouseMove(x, y);
                }
                return DefWindowProcW(hWnd, message, wParam, lParam);

            case WindowMessage.LeftButtonDown:
                if (ActiveCanvases.TryGetValue(hWnd, out canvas))
                {
                    var (x, y) = GetCoordinates(lParam);
                    canvas.OnLeftButtonDown(x, y);
                }
                return DefWindowProcW(hWnd, message, wParam, lParam);

            case WindowMessage.LeftButtonUp:
                if (ActiveCanvases.TryGetValue(hWnd, out canvas))
                {
                    var (x, y) = GetCoordinates(lParam);
                    canvas.OnLeftButtonUp(x, y);
                }
                return DefWindowProcW(hWnd, message, wParam, lParam);

            case WindowMessage.LeftButtonDoubleClick:
                if (ActiveCanvases.TryGetValue(hWnd, out canvas))
                {
                    var (x, y) = GetCoordinates(lParam);
                    canvas.OnLeftButtonDoubleClick(x, y);
                }
                return DefWindowProcW(hWnd, message, wParam, lParam);

            default:
                return DefWindowProcW(hWnd, message, wParam, lParam);
        }
    }
}

public enum WindowMessage : uint
{
    Null = 0x0000,
    MouseMove = 0x0200,
    LeftButtonDown = 0x0201,
    LeftButtonUp = 0x0202,
    LeftButtonDoubleClick = 0x0203,
    Mask = 0xFFFF,
}
