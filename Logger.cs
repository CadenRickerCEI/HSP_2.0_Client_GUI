using System;
using System.IO;

public class Logger
{
    private readonly string logDirectory;
    private readonly int maxLogFiles;
    private readonly long maxFileSizeInBytes;

    public Logger(string logDirectory, int maxLogFiles, long maxFileSizeInBytes)
    {
        this.logDirectory = logDirectory;
        this.maxLogFiles = maxLogFiles;
        this.maxFileSizeInBytes = maxFileSizeInBytes;
    }

    public void Log(string message)
    {
        string logFilePath = GetLogFilePath();
        if (File.Exists(logFilePath) && new FileInfo(logFilePath).Length > maxFileSizeInBytes)
        {
            RotateLogs();
        }

        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }

    private string GetLogFilePath()
    {
        return Path.Combine(logDirectory, "log.txt");
    }

    private void RotateLogs()
    {
        for (int i = maxLogFiles - 1; i > 0; i--)
        {
            string oldFile = Path.Combine(logDirectory, $"log{i}.txt");
            string newFile = Path.Combine(logDirectory, $"log{i + 1}.txt");

            if (File.Exists(oldFile))
            {
                File.Move(oldFile, newFile, true);
            }
        }

        File.Move(GetLogFilePath(), Path.Combine(logDirectory, "log1.txt"), true);
    }
}
/*
 * using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("This is a log message.");*/