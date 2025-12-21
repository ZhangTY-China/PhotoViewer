using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;

namespace PhotoViewer.Utils;

public static class Logger
{
    private static readonly string LogDirectory = Path.GetFullPath("C:\\PhotoViewerLogs");
    private const string LogFileName = "photoviewer_";
    private static readonly BlockingCollection<string> LogQueue = new();
    private static readonly Thread LogThread;

    static Logger()
    {
        Directory.CreateDirectory(LogDirectory);

        LogThread = new Thread(ProcessLogQueue)
        {
            IsBackground = true,
            Priority = ThreadPriority.Normal
        };
        LogThread.Start();

        AppDomain.CurrentDomain.ProcessExit += (_, _) => Shutdown();
    }

    private static void Shutdown()
    {
        LogQueue.CompleteAdding();
        LogThread.Join(1000);
    }

    public static void I(string tag, string message)
    {
        Log(tag, message, LogLevel.Info);
    }
    public static void E(string tag, string message)
    {
        Log(tag, message, LogLevel.Error);
    }
    public static void D(string tag, string message)
    {
        Log(tag, message, LogLevel.Debug);
    }
    public static void W(string tag, string message)
    {
        Log(tag, message, LogLevel.Warning);
    }

    private static void Log(string tag, string message, LogLevel level = LogLevel.Info)
    {
        try
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] [{tag}] {message}";
            LogQueue.Add(logEntry);
        }
        catch (Exception ex)
        {
            // 如果日志队列已满或关闭，直接写入错误日志
            try
            {
                File.AppendAllText(Path.Combine(LogDirectory, "logger_error.log"), 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [LoggerError] Failed to log: {ex.Message}{Environment.NewLine}");
            }
            catch { /* 防止无限循环 */ }
        }
    }

    private static void ProcessLogQueue()
    {
        var logFileName = LogFileName + DateTime.Now.ToString("yyyyMMdd") + ".log";
        var logPath = Path.Combine(LogDirectory, logFileName);

        foreach (var logEntry in LogQueue.GetConsumingEnumerable())
        {
            try
            {
                File.AppendAllText(logPath, logEntry + Environment.NewLine);

                // // 检查文件大小并执行轮转
                // var fileInfo = new FileInfo(logPath);
                // if (fileInfo.Length > MaxFileSize)
                // {
                //     RotateLogFiles();
                // }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(Path.Combine(LogDirectory, "logger_error.log"), 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [LoggerError] {ex}{Environment.NewLine}");
                }
                catch { /* 防止无限循环 */ }
            }
        }
    }

    public static void OpenLoggerInTerminal()
    {
        var logFileName = Logger.LogFileName + DateTime.Now.ToString("yyyyMMdd") + ".log";
        var logFilePath = Path.Combine(LogDirectory, logFileName);
        
        // 检查文件是否存在
        if (!File.Exists(logFilePath))
        {
            try
            {
                // 创建日志文件和目录（如果不存在）
                Directory.CreateDirectory(LogDirectory);
                File.WriteAllText(logFilePath, $"日志文件已创建: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法创建日志文件: {ex.Message}");
                return;
            }
        }
        
        // 根据操作系统选择合适的命令
        string command;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            // Windows系统使用PowerShell的Get-Content命令
            command = $"powershell -Command \"Get-Content '{logFilePath}' -Wait -Tail 20\"";
        }
        else
        {
            // Linux/Mac系统使用tail命令
            command = $"tail -f '{logFilePath}'";
        }
        
        ExecuteCommandInTerminal(command);
    }
    
    /// <summary>
    /// 在终端中执行命令
    /// </summary>
    /// <param name="command">要执行的命令</param>
    private static void ExecuteCommandInTerminal(string command)
    {
        try
        {
            // 创建新的进程
            ProcessStartInfo processInfo = new ProcessStartInfo();
            
            // 根据操作系统选择终端
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Windows系统
                processInfo.FileName = "cmd.exe";
                processInfo.Arguments = $"/c {command}";
            }
            else
            {
                // Linux/Mac系统
                processInfo.FileName = "/bin/bash";
                processInfo.Arguments = $"-c \"{command}\"";
            }
            
            // 设置进程属性
            processInfo.CreateNoWindow = false; // 显示终端窗口
            processInfo.UseShellExecute = true;
            
            // 启动进程
            Process.Start(processInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"执行命令时出错: {ex.Message}");
        }
    }
    
    private enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}