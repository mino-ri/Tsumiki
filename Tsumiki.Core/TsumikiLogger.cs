using System.Runtime.CompilerServices;
using System.Text;

namespace Tsumiki;
public static class TsumikiLogger
{
    private static readonly string _logPath = Path.Combine(Path.GetTempPath(), "tsumiki.txt");

    public static void WriteAccess(object[] args, [CallerMemberName] string caller = "")
    {
        try
        {
            var writer = new StringBuilder();
            writer.Append(DateTime.Now);
            writer.Append(' ');
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

    public static void WriteLog(string message)
    {
        try
        {
            var writer = new StringBuilder();
            writer.AppendLine($"{DateTime.Now} {message}");
            File.AppendAllText(_logPath, writer.ToString());
        }
        catch { }
    }

    public static void WriteException(Exception ex,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string caller = "")
    {
        try
        {
            var writer = new StringBuilder();
            writer.AppendLine($"{DateTime.Now} Error on {caller} [{filePath}:{lineNumber}]");
            writer.AppendLine(ex.ToString());
            File.AppendAllText(_logPath, writer.ToString());
        }
        catch { }
    }
}
