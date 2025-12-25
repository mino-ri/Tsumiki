using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Diagnostics.Tracing;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Xml;

namespace Tsumiki.View.Mac;

/// <summary>libTsumikiMetal へのP/Invokeインターフェース</summary>
internal static partial class MacInterop
{
    private const string LibName = "libTsumikiMetal";

    [LibraryImport(LibName)]
    public static partial nint tsumiki_renderer_create(nint nsView, int width, int height, nint shaderData, int shaderDataLength);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_destroy(nint handle);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_resize(nint handle, int width, int height);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_load_texture(nint handle, nint data, int width, int height);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_set_resource_image(nint handle, int type);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_clear(nint handle);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_draw_image(nint handle, in InteropRectF clientRange, in InteropRectF imageRange);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_draw_filter_graph(nint handle, in InteropRectF clientRange, float normalizedCutoff, float resonance);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_draw_modulator_graph(nint handle, in InteropRectF clientRange, in InteropGraphParameters parameters);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_draw_carrier_graph(nint handle, in InteropRectF clientRange, in InteropGraphParameters graphParams, in InteropFmParameters fmParams);

    [LibraryImport(LibName)]
    public static partial void tsumiki_renderer_present(nint handle);
}

[StructLayout(LayoutKind.Sequential)]
internal struct InteropRectF
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;

    public static implicit operator InteropRectF(in RectF source) => new()
    {
        Left = source.Left,
        Top = source.Top,
        Right = source.Right,
        Bottom = source.Bottom,
    };
}

[StructLayout(LayoutKind.Sequential)]
internal struct InteropGraphParameters
{
    public float Y;
    public float X;
    public float Pitch;
    public float Period;

    public static implicit operator InteropGraphParameters(in GraphParameters source) => new()
    {
        Y = source.Y,
        X = source.X,
        Pitch = source.Pitch,
        Period = source.Period,
    };
}

[StructLayout(LayoutKind.Sequential)]
internal struct InteropFmParameters
{
    public float X;
    public float Y;
    public float Pitch;
    public float Period;
    public float Level;
    public float _p0;
    public float _p1;
    public float _p2;

    public static implicit operator InteropFmParameters(in FmParameters source) => new()
    {
        X = source.X,
        Y = source.Y,
        Pitch = source.Pitch,
        Period = source.Period,
        Level = source.Level,
    };
}
