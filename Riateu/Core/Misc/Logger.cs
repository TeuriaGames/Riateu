using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SDL3;

namespace Riateu;

public static class Logger 
{
    public enum LogLevel { Debug, Warning, Error, Assert, Info }
    private static readonly StringBuilder writeLog = new StringBuilder();
    private static bool colored = true;
    public static LogLevel Verbosity = LogLevel.Info;
    private static void LogInternal(LogLevel level, string message, string cfp, int cln) 
    {
        if (Verbosity < level)
            return;
        
        var logName = level switch
        {
            LogLevel.Debug => "\u001b[92m[DEBUG]",
            LogLevel.Info => "\u001b[94m[INFO]",
            LogLevel.Error => "\u001b[91m[ERROR]",
            LogLevel.Warning => "\u001b[93m[WARNING]",
            LogLevel.Assert => "\u001b[91m[ASSERT]",
            _ => throw new InvalidOperationException()
        };
        var callSite = $"{Path.GetFileName(cfp)}:{cln}";

#if DEBUG
        {    
            Console.WriteLine(
                colored 
                ? $"\u001b[37m[{DateTime.Now.ToString("HH:mm:ss")}]{logName} {callSite} {message}" 
                : $"[{DateTime.Now.ToString("HH:mm:ss")}]{logName} {callSite} {message}"
            );
        }
#endif

        writeLog.AppendLine($"{logName}[{DateTime.Now.ToString("HH:mm:ss")}] {callSite} {message}");

        if (level == LogLevel.Error || level == LogLevel.Assert)
            Debugger.Break();
    }

    internal static void InitSDLLog() 
    {
        SDL.SDL_SetLogPriority((int) SDL.SDL_LogCategory.SDL_LOG_CATEGORY_GPU, SDL.SDL_LogPriority.SDL_LOG_PRIORITY_INFO);
        unsafe {
            SDL.SDL_SetLogOutputFunction(SDLLog, IntPtr.Zero);
        }
    }
    internal static unsafe void SDLLog(IntPtr userdata, int category, SDL3.SDL.SDL_LogPriority priority, byte* message) 
    {
        switch (priority) 
        {
        case SDL.SDL_LogPriority.SDL_LOG_PRIORITY_INFO:
            Info(Marshal.PtrToStringUTF8((IntPtr)message));
            break;
        case SDL.SDL_LogPriority.SDL_LOG_PRIORITY_WARN:
            Warn(Marshal.PtrToStringUTF8((IntPtr)message));
            break;
        case SDL.SDL_LogPriority.SDL_LOG_PRIORITY_ERROR:
            Error(Marshal.PtrToStringUTF8((IntPtr)message));
            break;
        }
    }

    public static void Print(string messageToPrint) 
    {
        Console.Write(messageToPrint);
    } 

    public static void Log(
        string log, 
        LogLevel logLevel = LogLevel.Debug,
        [CallerFilePath] string callerFilePath = "", 
        [CallerLineNumber] int callerLineNumber = 0
    ) 
    {
        LogInternal(logLevel, log, callerFilePath, callerLineNumber);
    }

    public static void Log(
        object log,
        LogLevel logLevel = LogLevel.Debug,
        [CallerFilePath] string callerFilePath = "", 
        [CallerLineNumber] int callerLineNumber = 0
    ) 
    {
        string message = log switch 
        {
            null => "null",
            _ => log.ToString() ?? "null"
        };
        LogInternal(logLevel, message, callerFilePath, callerLineNumber);
    }

    public static void Info(
        string log, 
        [CallerFilePath] string callerFilePath = "", 
        [CallerLineNumber] int callerLineNumber = 0
    ) 
    {
        LogInternal(LogLevel.Info, log, callerFilePath, callerLineNumber);
    }

    [Conditional("DEBUG")]
    public static void Assert(
        bool condition, 
        string message, 
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0
    ) 
    {
        if (!condition)
            LogInternal(LogLevel.Assert, message, callerFilePath, callerLineNumber);
    }

    public static void Warn(
        string log, 
        [CallerFilePath] string callerFilePath = "", 
        [CallerLineNumber] int callerLineNumber = 0
    ) 
    {
        LogInternal(LogLevel.Warning, log, callerFilePath, callerLineNumber);
    }

    public static void Error(
        string log, 
        [CallerFilePath] string callerFilePath = "", 
        [CallerLineNumber] int callerLineNumber = 0
    ) 
    {
        LogInternal(LogLevel.Error, log, callerFilePath, callerLineNumber);
    }

    public static void Error(
        object log, 
        [CallerFilePath] string callerFilePath = "", 
        [CallerLineNumber] int callerLineNumber = 0
    ) 
    {
        string message = log switch 
        {
            null => "null",
            _ => log.ToString() ?? "null"
        };
        
        LogInternal(LogLevel.Error, message, callerFilePath, callerLineNumber);
    }

    public static void OpenLog(string path) 
    {
        var process = new Process() 
        {
            StartInfo = new ProcessStartInfo(path) 
            {
                UseShellExecute = true
            }
        };
        if (File.Exists(path))
            process.Start();
    }

    public static void WriteToFile(string path) 
    {
        var directory = Path.GetDirectoryName(path);
        if (directory != null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        WriteToFile(fs);
    }

    public static void WriteToFile(Stream stream) 
    {
        using var textWriter = new StreamWriter(stream);
        textWriter.WriteLine(writeLog.ToString());
    }

    public static async Task WriteToFileAsync(string path, CancellationToken token = default) 
    {
        var directory = Path.GetDirectoryName(path);
        if (directory != null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        await WriteToFileAsync(fs, token);
    }

    public static async Task WriteToFileAsync(Stream stream, CancellationToken token = default) 
    {
        using var textWriter = new StreamWriter(stream);
        await textWriter.WriteLineAsync(writeLog.ToString().AsMemory(), token);
    }
}