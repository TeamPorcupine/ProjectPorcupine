using System.Collections;
using System.Collections.Generic;
using UberLogger;
using System.IO;
using UnityEngine;

/// <summary>
/// A basic file logger backend
/// </summary>
public class UberLoggerFile : UberLogger.ILogger
{
    private StreamWriter LogFileWriter;
    private bool IncludeCallStacks;

    /// <summary>
    /// Constructor. Make sure to add it to UberLogger via Logger.AddLogger();
    /// filename is relative to Application.persistentDataPath
    /// if includeCallStacks is true it will dump out the full callstack for all logs, at the expense of big log files.
    /// </summary>
    public UberLoggerFile(string filename, bool includeCallStacks = true)
    {
        IncludeCallStacks = includeCallStacks;
        var fileLogPath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        Debug.Log("Initialising file logging to " + fileLogPath);
        LogFileWriter = new StreamWriter(fileLogPath, false);
        LogFileWriter.AutoFlush = true;
    }

    public void Log(LogInfo logInfo)
    {
        lock(this)
        {
            LogFileWriter.WriteLine(logInfo.Message);
            if(IncludeCallStacks && logInfo.Callstack.Count>0)
            {
                foreach(var frame in logInfo.Callstack)
                {
                    LogFileWriter.WriteLine(frame.GetFormattedMethodName());
                }
                LogFileWriter.WriteLine();
            }
        }
    }
}
