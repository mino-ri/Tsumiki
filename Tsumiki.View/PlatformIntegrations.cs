using System;
using System.Runtime.InteropServices;
using Avalonia.Controls.Embedding;

namespace Tsumiki.View;
internal static partial class PlatformIntegrations
{
#if OSX
#else
    [LibraryImport("user32.dll")]
    private static partial IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
#endif

    internal static void AttackParent(EmbeddableControlRoot controlRoot, IntPtr parentHandle)
    {
#if OSX
#else
        if (controlRoot.TryGetPlatformHandle() is { } handle)
        {
            SetParent(handle.Handle, parentHandle);
        }
#endif
    }
}
