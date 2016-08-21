#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine;

public static class Logger
{
    public enum Level
    {
        Exception,
        Assertion,
        Error,
        Warning,
        Info,
        Verbose
    }

    // TODO: Not used yet but it's the logical next step.
    public enum Channel
    {
        Default
    }

    static Level minimumLevel = Level.Info;

    public static void LogException(Exception exception)
    {
        // Exceptions are special as they support neither formatting nor message
        if (minimumLevel <= Level.Exception)
        {
            Debug.LogException(exception);
        }
    }

    public static void LogAssertion(object message)
    {
        Log(Level.Assertion, message);
    }

    public static void LogAssertionFormat(string format, params object[] args)
    {
        Log(Level.Assertion, format, args);
    }

    public static void LogError(object message)
    {
        Log(Level.Error, message);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        Log(Level.Error, format, args);
    }

    public static void LogWarning(object message)
    {
        Log(Level.Warning, message);
    }

    public static void LogWarningFormat(string format, params object[] args)
    {
        Log(Level.Warning, format, args);
    }

    public static void LogInfo(object message)
    {
        Log(Level.Info, message);
    }

    public static void LogInfoFormat(string format, params object[] args)
    {
        Log(Level.Info, format, args);
    }

    public static void LogVerbose(object message)
    {
        Log(Level.Verbose, message);
    }

    public static void LogVerboseFormat(string format, params object[] args)
    {
        Log(Level.Verbose, format, args);
    }

    /// <summary>
    /// Alias for info.
    /// </summary>
    public static void Log(object message)
    {
        Log(Level.Info, message);
    }

    /// <summary>
    /// Alias for info.
    /// </summary>
    public static void LogFormat(string message, params object[] args)
    {
        Log(Level.Info, message, args);
    }

    private static void Log(Level level, object message)
    {
        // This method just formats the messsage, but just in case ToString is expensive on this object we check level first.
        if (level > minimumLevel)
        {
            return;
        }

        Log(level, message == null ? null : message.ToString());
    }

    private static void Log(Level level, string message, params object[] formatArgs)
    {
        if (level > minimumLevel)
        {
            return;
        }

        if (formatArgs.Length > 0)
        {
            message = string.Format(message, formatArgs);
        }

        switch (level)
        {
            case Level.Exception:
                throw new InvalidOperationException("Exceptions are logged in a special way. You shouldnt see this message");
            case Level.Assertion:
                Debug.LogAssertion(message);
                break;
            case Level.Error:
                Debug.LogError(message);
                break;
            case Level.Warning:
                Debug.LogWarning(message);
                break;
            case Level.Info:
                Debug.Log(message);
                break;
            case Level.Verbose:
                Debug.Log(message);
                break;
            default:
                throw new ArgumentException("Unhandled level: " + level, "level");    
        }
    }
}
