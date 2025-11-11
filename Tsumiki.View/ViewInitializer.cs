using System;
using System.Threading;
using Avalonia;

namespace Tsumiki.View;
public static class ViewInitializer
{
    private static int InitializeCount = 0;

    public static void Initialize()
    {
        if (Interlocked.Increment(ref InitializeCount) == 1)
        {
            AppBuilder.Configure<App>()
                //.UsePlatformDetect()
                .UseWin32()
                .UseSkia()
                .WithInterFont()
                .SetupWithoutStarting();
        }
    }

    public static ControlRoot AttackInParentWindow(IntPtr parentHandle)
    {
        Initialize();

        var root = new ControlRoot();
        PlatformIntegrations.AttackParent(root, parentHandle);
        root.Prepare();
        root.StartRendering();
        return root;
    }
}
