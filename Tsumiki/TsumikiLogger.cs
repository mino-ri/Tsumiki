using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Tsumiki;
internal static class TsumikiLogger
{
    private static readonly string _logPath = Path.Combine(Path.GetTempPath(), "tsumiki.txt");

    internal static void WriteAccess(object[] args, [CallerMemberName] string caller = "")
    {
        try
        {
            var writer = new StringBuilder();
            writer.Append(DateTime.Now);
            writer.Append(caller);
            foreach (var arg in args)
            {
                writer.Append($" {arg}");
            }
            writer.AppendLine();
            File.AppendAllText(_logPath, writer.ToString());
        }
        catch { }
    }

    internal static void WriteLog(string message)
    {
        try
        {
            var writer = new StringBuilder();
            writer.Append(DateTime.Now);
            writer.AppendLine(message);
            File.AppendAllText(_logPath, writer.ToString());
        }
        catch { }
    }
}
