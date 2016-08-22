using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UberLogger
{
    //Use this to exclude methods from the stack trace
    [AttributeUsage(AttributeTargets.Method)]
    public class StackTraceIgnore : Attribute {}

    //Use this to stop UberLogger handling logs with this in the callstack.
    [AttributeUsage(AttributeTargets.Method)]
    public class LogUnityOnly : Attribute {}

    public enum LogSeverity
    {
        Message,
        Warning,
        Error,
    }

    /// <summary>
    /// Interface for deriving new logger backends.
    /// Add a new logger via Logger.AddLogger()
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logging backend entry point. logInfo contains all the information about the logging request.
        /// </summary>
        void Log(LogInfo logInfo);
    }
    

    //Information about a particular frame of a callstack
    [System.Serializable]
    public class LogStackFrame
    {
        public string MethodName;
        public string DeclaringType;
        public string ParameterSig;

        public int LineNumber;
        public string FileName;

        string FormattedMethodName;

        /// <summary>
        /// Convert from a .Net stack frame
        /// </summary>
        public LogStackFrame(StackFrame frame)
        {
            var method = frame.GetMethod();
            MethodName = method.Name;
            DeclaringType = method.DeclaringType.Name;

            var pars = method.GetParameters();
            for (int c1=0; c1<pars.Length; c1++)
            {
                ParameterSig += String.Format("{0} {1}", pars[c1].ParameterType, pars[c1].Name);
                if(c1+1<pars.Length)
                {
                    ParameterSig += ", ";
                }
            }

            FileName = frame.GetFileName();
            LineNumber = frame.GetFileLineNumber();
            FormattedMethodName = MakeFormattedMethodName();
        }

        /// <summary>
        /// Convert from a Unity stack frame (for internal Unity errors rather than excpetions)
        /// </summary>
        public LogStackFrame(string unityStackFrame)
        {
            if(Logger.ExtractInfoFromUnityStackInfo(unityStackFrame, ref DeclaringType, ref MethodName, ref FileName, ref LineNumber))
            {
                FormattedMethodName = MakeFormattedMethodName();
            }
            else
            {
                FormattedMethodName = unityStackFrame;
            }
        }


        /// <summary>
        /// Basic stack frame info when we have nothing else
        /// </summary>
        public LogStackFrame(string message, string filename, int lineNumber)
        {
            FileName = filename;
            LineNumber = lineNumber;
            FormattedMethodName = message;
        }



        public string GetFormattedMethodName()
        {
            return FormattedMethodName;
        }

        /// <summary>
        /// Make a nice string showing the stack information - used by the loggers
        /// </summary>
        string MakeFormattedMethodName()
        {
            string filename = FileName;
            if(!String.IsNullOrEmpty(FileName))
            {
                var startSubName = FileName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);

                if(startSubName>0)
                {
                    filename = FileName.Substring(startSubName);
                }
            }
            string methodName = String.Format("{0}.{1}({2}) (at {3}:{4})", DeclaringType, MethodName, ParameterSig, filename, LineNumber);
            return methodName;
        }
    }

    /// <summary>
    /// A single item of logging information
    /// </summary>
    [System.Serializable]
    public class LogInfo
    {
        public UnityEngine.Object Source;
        public string Channel;
        public LogSeverity Severity;
        public string Message;
        public List<LogStackFrame> Callstack;
        public double TimeStamp;
        string TimeStampAsString;

        public string GetTimeStampAsString()
        {
            return TimeStampAsString;
        }

        public LogInfo(UnityEngine.Object source, string channel, LogSeverity severity, List<LogStackFrame> callstack, object message, params object[] par)
        {
            Source = source;
            Channel = channel;
            Severity = severity;
            Message = "";

            var messageString = message as String;
            if(messageString!=null)
            {
                if(par.Length>0)
                {
                    Message = System.String.Format(messageString, par);
                }
                else
                {
                    Message = messageString;
                }
            }
            else
            {
                if(message!=null)
                {
                    Message = message.ToString();
                }
            }

            Callstack = callstack;
            TimeStamp = Logger.GetTime();
            TimeStampAsString = String.Format("{0:0.0000}", TimeStamp);
        }
    }

    /// <summary>
    /// The core of UberLogger - the entry point for logging information
    /// </summary>
    public static class Logger
    {
        // Controls how many historical messages to keep to pass into newly registered loggers
        public static int MaxMessagesToKeep = 1000;

        // If true, any logs sent to UberLogger will be forwarded on to the Unity logger.
        // Useful if you want to use both systems
        public static bool ForwardMessages = true;

        static List<ILogger> Loggers = new List<ILogger>();
        static LinkedList<LogInfo> RecentMessages = new LinkedList<LogInfo>();
        static double StartTime;
        static bool AlreadyLogging = false;

        static Logger()
        {
            // Register with Unity's logging system
#if UNITY_5
            Application.logMessageReceived += UnityLogHandler;
#else
            Application.RegisterLogCallback(UnityLogHandler);
#endif
            StartTime = GetTime();
        }

        /// <summary>
        /// Registered Unity error handler
        /// </summary>
        [StackTraceIgnore]
        static void UnityLogHandler(string logString, string stackTrace, UnityEngine.LogType logType)
        {
            UnityLogInternal(logString, stackTrace, logType);
        }
    
        static public double GetTime()
        {
#if UNITY_EDITOR
            return EditorApplication.timeSinceStartup - StartTime;
#else
            double time = Time.time;
            return time - StartTime;
#endif
        }

        /// <summary>
        /// Registers a new logger backend, which we be told every time there's a new log.
        /// if populateWithExistingMessages is true, UberLogger will immediately pump the new logger with past messages
        /// </summary>
        static public void AddLogger(ILogger logger, bool populateWithExistingMessages=true)
        {
            lock(Loggers)
            {
                if(populateWithExistingMessages)
                {
                    foreach(var oldLog in RecentMessages)
                    {
                        logger.Log(oldLog);
                    }
                }

                if(!Loggers.Contains(logger))
                {
                    Loggers.Add(logger);
                }
            }
        }

        /// <summary>
        /// Tries to extract useful information about the log from a Unity error message.
        /// Only used when handling a Unity error message and we can't get a useful callstack
        /// </summary>
        static public bool ExtractInfoFromUnityMessage(string log, ref string filename, ref int lineNumber)
        {
            // log = "Assets/Code/Debug.cs(140,21): warning CS0618: 'some error'
            var match = System.Text.RegularExpressions.Regex.Matches(log, @"(.*)\((\d+).*\)");

            if(match.Count>0)
            {
                filename = match[0].Groups[1].Value;
                lineNumber = Convert.ToInt32(match[0].Groups[2].Value);
                return true;
            }
            return false;
        }
    

        /// <summary>
        /// Tries to extract useful information about the log from a Unity stack trace
        /// </summary>
        static public bool ExtractInfoFromUnityStackInfo(string log, ref string declaringType, ref string methodName, ref string filename, ref int lineNumber)
        {
            // log = "DebugLoggerEditorWindow.DrawLogDetails () (at Assets/Code/Editor.cs:298)";
            var match = System.Text.RegularExpressions.Regex.Matches(log, @"(.*)\.(.*)\s*\(.*\(at (.*):(\d+)");

            if(match.Count>0)
            {
                declaringType = match[0].Groups[1].Value;
                methodName = match[0].Groups[2].Value;
                filename = match[0].Groups[3].Value;
                lineNumber = Convert.ToInt32(match[0].Groups[4].Value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts the curent stack trace into a list of UberLogger's LogStackFrame.
        /// Excludes any methods with the StackTraceIgnore attribute
        /// Returns false if the stack frame contains any methods flagged as LogUnityOnly
        /// </summary>
        [StackTraceIgnore]
        static bool GetCallstack(ref List<LogStackFrame> callstack)
        {
            callstack.Clear();
            StackTrace stackTrace = new StackTrace(true);           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            foreach (StackFrame stackFrame in stackFrames)
            {
                var method = stackFrame.GetMethod();
                if(method.IsDefined(typeof(LogUnityOnly), true))
                {
                    return true;
                }
                if(!method.IsDefined(typeof(StackTraceIgnore), true))
                {
                    //Cut out some internal noise from Unity stuff
                    if(!(method.Name=="CallLogCallback" && method.DeclaringType.Name=="Application")
                    && !(method.DeclaringType.Name=="Debug" && (method.Name=="Internal_Log" || method.Name=="Log")))
                    {
                        var logStackFrame = new LogStackFrame(stackFrame);
                        
                        callstack.Add(logStackFrame);
                        
                    }
                }
            } 
        
            return false;
        }

        /// <summary>
        /// Converts a Unity callstack string into a list of UberLogger's LogStackFrame
        /// Doesn't do any filtering, since this should only be dealing with internal Unity errors rather than client code
        /// </summary>
        static List<LogStackFrame> GetCallstackFromUnityLog(string unityCallstack)
        {
            var lines = System.Text.RegularExpressions.Regex.Split(unityCallstack, System.Environment.NewLine); 
            var stack = new List<LogStackFrame>();
            foreach(var line in lines)
            {
                var frame = new LogStackFrame(line);
                if(!string.IsNullOrEmpty(frame.GetFormattedMethodName()))
                {
                    stack.Add(new LogStackFrame(line));
                }
            }
            return stack;
        }

        /// <summary>
        /// The core entry point of all logging coming from Unity. Takes a log request, creates the call stack and pumps it to all the backends
        /// </summary>
        [StackTraceIgnore()]
        static void UnityLogInternal(string unityMessage, string unityCallStack, UnityEngine.LogType logType)
        {
            //Make sure only one thread can do this at a time.
            //This should mean that most backends don't have to worry about thread safety (unless they do complicated stuff)
            lock(Loggers)
            {
                //Prevent nasty recursion problems
                if(!AlreadyLogging)
                {
                    try
                    {
                        AlreadyLogging = true;
                    
                        var callstack = new List<LogStackFrame>();
                        var unityOnly = GetCallstack(ref callstack);
                        if(unityOnly)
                        {
                            return;
                        }

                        //If we have no useful callstack, fall back to parsing Unity's callstack 
                        if(callstack.Count==0)
                        {
                            callstack = GetCallstackFromUnityLog(unityCallStack);
                        }

                        LogSeverity severity;
                        switch(logType)
                        {
                            case UnityEngine.LogType.Error: severity = LogSeverity.Error; break;
                            case UnityEngine.LogType.Exception: severity = LogSeverity.Error; break;
                            case UnityEngine.LogType.Warning: severity = LogSeverity.Warning; break;
                            default: severity = LogSeverity.Message; break;
                        }

                        string filename = "";
                        int lineNumber = 0;
                    
                        //Finally, parse the error message so we can get basic file and line information
                        if(ExtractInfoFromUnityMessage(unityMessage, ref filename, ref lineNumber))
                        {
                            callstack.Insert(0, new LogStackFrame(unityMessage, filename, lineNumber));
                        }

                        var logInfo = new LogInfo(null, "", severity, callstack, unityMessage);

                        //Add this message to our history
                        RecentMessages.AddLast(logInfo);

                        //Make sure our history doesn't get too big
                        TrimOldMessages();

                        //Delete any dead loggers and pump them with the new log
                        Loggers.RemoveAll(l=>l==null);
                        Loggers.ForEach(l=>l.Log(logInfo));
                    }
                    finally
                    {
                        AlreadyLogging = false;
                    }
                }
            }
        }


        /// <summary>
        /// The core entry point of all logging coming from client code.
        /// Takes a log request, creates the call stack and pumps it to all the backends
        /// </summary>
        [StackTraceIgnore()]
        static public void Log(string channel, UnityEngine.Object source, LogSeverity severity, object message, params object[] par)
        {
            lock(Loggers)
            {
                if(!AlreadyLogging)
                {
                    try
                    {
                        AlreadyLogging = true;
                        var callstack = new List<LogStackFrame>();
                        var unityOnly = GetCallstack(ref callstack);
                        if(unityOnly)
                        {
                            return;
                        }

                        var logInfo = new LogInfo(source, channel, severity, callstack, message, par);

                        //Add this message to our history
                        RecentMessages.AddLast(logInfo);

                        //Make sure our history doesn't get too big
                        TrimOldMessages();

                        //Delete any dead loggers and pump them with the new log
                        Loggers.RemoveAll(l=>l==null);
                        Loggers.ForEach(l=>l.Log(logInfo));

                        //If required, pump this message back into Unity
                        if(ForwardMessages)
                        {
                            ForwardToUnity(source, severity, message, par);
                        }
                    }
                    finally
                    {
                        AlreadyLogging = false;
                    }
                }
            }
        }

        /// <summary>
        /// Forwards an UberLogger log to Unity so it's visible in the built-in console
        /// </summary>
        [LogUnityOnly()]
        static void ForwardToUnity(UnityEngine.Object source, LogSeverity severity, object message, params object[] par)
        {
			object showObject = null;
            if(message!=null)
            {
				var messageAsString = message as string;
				if(messageAsString!=null)
				{
	                if(par.Length>0)
    	            {
        	            showObject = String.Format(messageAsString, par);
            	    }
            	    else
               		{
                    	showObject = message;
                	}
				}
				else
				{
					showObject = message;
				}
            }

            if(source==null)
            {
				if(severity==LogSeverity.Message) UnityEngine.Debug.Log(showObject);
                else if(severity==LogSeverity.Warning) UnityEngine.Debug.LogWarning(showObject);
                else if(severity==LogSeverity.Error) UnityEngine.Debug.LogError(showObject);
            }
            else
            {
                if(severity==LogSeverity.Message) UnityEngine.Debug.Log(showObject, source);
                else if(severity==LogSeverity.Warning) UnityEngine.Debug.LogWarning(showObject, source);
                else if(severity==LogSeverity.Error) UnityEngine.Debug.LogError(showObject, source);
            }
        }

        /// <summary>
        /// Finds a registered logger, if it exists
        /// </summary>
        static public T GetLogger<T>() where T:class
        {
            foreach(var logger in Loggers)
            {
                if(logger is T)
                {
                    return logger as T;
                }
            }
            return null;
        }

        static void TrimOldMessages()
        {
            while(RecentMessages.Count > MaxMessagesToKeep)
            {
                RecentMessages.RemoveFirst();
            }
        }
    }

}
