using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UberLogger;

/// <summary>
/// The console logging frontend.
/// Pulls data from the UberLoggerEditor backend
/// </summary>

public class UberLoggerEditorWindow : EditorWindow
{
    [MenuItem("Window/Show Uber Console")]
    static public void ShowLogWindow()
    {
        Init();
    }

    static public void Init()
    {
        var window = ScriptableObject.CreateInstance<UberLoggerEditorWindow>();
        window.Show();
        window.position = new Rect(200,200,400,300);
        window.CurrentTopPaneHeight = window.position.height/2;
    }

    void OnEnable()
    {
        // Connect to or create the backend
        if(!EditorLogger)
        {
            EditorLogger = UberLogger.Logger.GetLogger<UberLoggerEditor>();
            if(!EditorLogger)
            {
                EditorLogger = UberLoggerEditor.Create();
            }
        }

        // UberLogger doesn't allow for duplicate loggers, so this is safe
        // And, due to Unity serialisation stuff, necessary to do to it here.
        UberLogger.Logger.AddLogger(EditorLogger);
        
#if UNITY_5
        titleContent.text = "Uber Console";
#else
        title = "Uber Console";

#endif
         
        ClearSelectedMessage();

        SmallErrorIcon = EditorGUIUtility.FindTexture( "d_console.erroricon.sml" ) ;
        SmallWarningIcon = EditorGUIUtility.FindTexture( "d_console.warnicon.sml" ) ;
        SmallMessageIcon = EditorGUIUtility.FindTexture( "d_console.infoicon.sml" ) ;
        ErrorIcon = SmallErrorIcon;
        WarningIcon = SmallWarningIcon;
        MessageIcon = SmallMessageIcon;
    }

    public void OnGUI()
    {
        //Set up the basic style, based on the Unity defaults
        //A bit hacky, but means we don't have to ship an editor guistyle and can fit in to pro and free skins
        Color defaultLineColor = GUI.backgroundColor;
        
        foreach(var style in GUI.skin.customStyles)
        {
            if(style.name=="LODSliderRangeSelected")
            {
                SelectedLogLineStyle = new GUIStyle(EditorStyles.label);
                LogLineStyle = new GUIStyle(EditorStyles.label);
                SelectedLogLineStyle.margin = new RectOffset(0, 0, 0,0 );
                SelectedLogLineStyle.normal.background = style.normal.background;
                SelectedLogLineStyle.active = SelectedLogLineStyle.normal;
                SelectedLogLineStyle.hover = SelectedLogLineStyle.normal;
                SelectedLogLineStyle.focused = SelectedLogLineStyle.normal;
                
                LogLineStyle.margin = new RectOffset(0, 0, 0,0 );
                LogLineStyle.normal.background = EditorGUIUtility.whiteTexture;
                LogLineStyle.active = LogLineStyle.normal;
                LogLineStyle.hover = LogLineStyle.normal;
                LogLineStyle.focused = LogLineStyle.normal;
                break;
            }
        }

        LineColour1 = defaultLineColor;
        LineColour2 = new Color(defaultLineColor.r*0.9f, defaultLineColor.g*0.9f, defaultLineColor.b*0.9f);
        SizerLineColour = new Color(defaultLineColor.r*0.5f, defaultLineColor.g*0.5f, defaultLineColor.b*0.5f);

        GUILayout.BeginVertical(GUILayout.Height(CurrentTopPaneHeight), GUILayout.MinHeight(100));
        DrawToolbar();
        DrawFilter();
        DrawChannels();
        DrawLogList();
        GUILayout.EndVertical();
        ResizeTopPane();

        //Create a small gap so the resize handle isn't overwritten
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        DrawLogDetails();
        GUILayout.EndVertical();

        //Force a repaint, since we're constantly resizing/adding stuff to the window
        //Potential optimisation here is to only repaint if the window is resized or we have new logs
        Repaint();
    }

    //Some helper functions to draw buttons that are only as big as their text
    bool ButtonClamped(string text, GUIStyle style)
    {
        return GUILayout.Button(text, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x));
    }

    bool ToggleClamped(bool state, string text, GUIStyle style)
    {
        return GUILayout.Toggle(state, text, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x));
    }

    bool ToggleClamped(bool state, GUIContent content, GUIStyle style, params GUILayoutOption[] par)
    {
        return GUILayout.Toggle(state, content, style, GUILayout.MaxWidth(style.CalcSize(content).x));
    }

    void LabelClamped(string text, GUIStyle style)
    {
        GUILayout.Label(text, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(text)).x));
    }

    /// <summary>
    /// Draws the thin, Unity-style toolbar showing error counts and toggle buttons
    /// </summary>
    void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();
        if(ButtonClamped("Clear", EditorStyles.toolbarButton))
        {
            EditorLogger.Clear();
        }
        EditorLogger.ClearOnPlay = ToggleClamped(EditorLogger.ClearOnPlay, "Clear On Play", EditorStyles.toolbarButton);
        EditorLogger.PauseOnError  = ToggleClamped(EditorLogger.PauseOnError, "Pause On Error", EditorStyles.toolbarButton);
        ShowTimes = ToggleClamped(ShowTimes, "Show Times", EditorStyles.toolbarButton);

        var buttonSize = EditorStyles.toolbarButton.CalcSize(new GUIContent("T")).y;
        GUILayout.FlexibleSpace();

        var showErrors = ToggleClamped(ShowErrors, new GUIContent(EditorLogger.NoErrors.ToString(), SmallErrorIcon), EditorStyles.toolbarButton, GUILayout.Height(buttonSize));
        var showWarnings = ToggleClamped(ShowWarnings, new GUIContent(EditorLogger.NoWarnings.ToString(), SmallWarningIcon), EditorStyles.toolbarButton, GUILayout.Height(buttonSize));
        var showMessages = ToggleClamped(ShowMessages, new GUIContent(EditorLogger.NoMessages.ToString(), SmallMessageIcon), EditorStyles.toolbarButton, GUILayout.Height(buttonSize));
        //If the errors/warning to show has changed, clear the selected message
        if(showErrors!=ShowErrors || showWarnings!=ShowWarnings || showMessages!=ShowMessages)
        {
            ClearSelectedMessage();
        }
        ShowWarnings = showWarnings;
        ShowMessages = showMessages;
        ShowErrors = showErrors;
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws the channel selector
    /// </summary>
    void DrawChannels()
    {
        var channels = GetChannels();
        int currentChannelIndex = 0;
        for(int c1=0; c1<channels.Count; c1++)
        {
            if(channels[c1]==CurrentChannel)
            {
                currentChannelIndex = c1;
                break;
            }
        }

        currentChannelIndex = GUILayout.SelectionGrid(currentChannelIndex, channels.ToArray(), channels.Count);
        if(CurrentChannel!=channels[currentChannelIndex])
        {
            CurrentChannel = channels[currentChannelIndex];
            ClearSelectedMessage();
        }
    }

    /// <summary>
    /// Based on filter and channel selections, should this log be shown?
    /// </summary>
    bool ShouldShowLog(System.Text.RegularExpressions.Regex regex, LogInfo log)
    {
        if(log.Channel==CurrentChannel || CurrentChannel=="All" || (CurrentChannel=="No Channel" && String.IsNullOrEmpty(log.Channel)))
        {
            if((log.Severity==LogSeverity.Message && ShowMessages)
               || (log.Severity==LogSeverity.Warning && ShowWarnings)
               || (log.Severity==LogSeverity.Error && ShowErrors))
            {
                if(regex==null || regex.IsMatch(log.Message))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// Draws the main log panel
    /// </summary>
    public void DrawLogList()
    {
        var oldColor = GUI.backgroundColor;

        LogListScrollPosition = EditorGUILayout.BeginScrollView(LogListScrollPosition);
        var maxLogPanelHeight = position.height;
                
        float buttonY = 0;
        float buttonHeight = LogLineStyle.CalcSize(new GUIContent("Test")).y;
        
        System.Text.RegularExpressions.Regex filterRegex = null;

        if(!String.IsNullOrEmpty(FilterRegex))
        {
            filterRegex = new System.Text.RegularExpressions.Regex(FilterRegex);
        }

        int drawnButtons = 0;
        var logLineStyle = new GUIStyle(LogLineStyle);
        for(int c1=0; c1<EditorLogger.LogInfo.Count; c1++)
        {
            var log = EditorLogger.LogInfo[c1];
            if(ShouldShowLog(filterRegex, log))
            {
                drawnButtons++;

                //This is an optimisation - if the button isn't going to display because it's outside of the scroll window, don't show it.
                //But, so as not to confuse GUILayout, draw something simple instead.
                if(buttonY+buttonHeight>LogListScrollPosition.y && buttonY<LogListScrollPosition.y+maxLogPanelHeight)
                {
                    if(c1==SelectedMessage)
                    {
                        logLineStyle = SelectedLogLineStyle;
                        GUI.backgroundColor = Color.white;
                    }
                    else
                    {
                        logLineStyle = LogLineStyle;
                        GUI.backgroundColor = (drawnButtons%2==0) ? LineColour1 : LineColour2;
                    }
                
                    var showMessage = log.Message;

                    //Make all messages single line
                    showMessage = showMessage.Replace(System.Environment.NewLine, " ");
                    if(ShowTimes)
                    {
                        showMessage = log.GetTimeStampAsString() + ": " + showMessage; 
                    }

                    var content = new GUIContent(showMessage, GetIconForLog(log));
                    if(GUILayout.Button(content, logLineStyle, GUILayout.Height(buttonHeight)))
                    {
                        //Select a message, or jump to source if it's double-clicked
                        if(c1==SelectedMessage)
                        {
                            if(EditorApplication.timeSinceStartup-LastMessageClickTime<0.3f)
                            {
                                LastMessageClickTime = 0;
                                if(log.Callstack.Count>0)
                                {
                                    JumpToSource(log.Callstack[0]);
                                }
                            }
                            else
                            {
                                LastMessageClickTime = EditorApplication.timeSinceStartup;
                            }
                        }
                        else
                        {
                            SelectedMessage = c1;
                            SelectedCallstackFrame = -1;
                        }

                        //Always select the game object that is the source of this message
                        var go = log.Source as GameObject;
                        if(go!=null)
                        {
                            Selection.activeGameObject = go;
                        }

                    }
                }
                else
                {
                    GUILayout.Space(buttonHeight);
                }
                buttonY += buttonHeight;
            }
        }
        EditorGUILayout.EndScrollView();
        GUI.backgroundColor = oldColor;
    }


    /// <summary>
    /// The bottom of the panel - details of the selected log
    /// </summary>
    public void DrawLogDetails()
    {
        var oldColor = GUI.backgroundColor;
        SelectedMessage = Mathf.Clamp(SelectedMessage, 0, EditorLogger.LogInfo.Count);
        if(EditorLogger.LogInfo.Count>0 && SelectedMessage>=0)
        {
            LogDetailsScrollPosition = EditorGUILayout.BeginScrollView(LogDetailsScrollPosition);
            var log = EditorLogger.LogInfo[SelectedMessage];
            var logLineStyle = LogLineStyle;
            for(int c1=0; c1<log.Callstack.Count; c1++)
            {
                var frame = log.Callstack[c1];
                var methodName = frame.GetFormattedMethodName();
                if(!String.IsNullOrEmpty(methodName))
                {
                    if(c1==SelectedCallstackFrame)
                    {
                        GUI.backgroundColor = Color.white;
                        logLineStyle = SelectedLogLineStyle;
                    }
                    else
                    {
                        GUI.backgroundColor = (c1%2==0) ? LineColour1 : LineColour2;
                        logLineStyle = LogLineStyle;
                    }
                    

                    // Handle clicks on the stack frame
                    if(GUILayout.Button(methodName, logLineStyle))
                    {
                        if(c1==SelectedCallstackFrame)
                        {
                            if(Event.current.button==1)
                            {
                                ToggleShowSource(frame);
                            }
                            else
                            {
                                if(EditorApplication.timeSinceStartup-LastFrameClickTime<0.3f)
                                {
                                    LastFrameClickTime = 0;
                                    JumpToSource(frame);
                                }
                                else
                                {
                                    LastFrameClickTime = EditorApplication.timeSinceStartup;
                                }
                            }
                            
                        }
                        else
                        {
                            SelectedCallstackFrame = c1;
                        }
                    }
                    if(ShowFrameSource && c1==SelectedCallstackFrame)
                    {
                        DrawFrameSource(frame);
                    }
                    
                }

            }
            EditorGUILayout.EndScrollView();
        }
        GUI.backgroundColor = oldColor;
    }

    Texture2D GetIconForLog(LogInfo log)
    {
        if(log.Severity==LogSeverity.Error)
        {
            return ErrorIcon;
        }
        if(log.Severity==LogSeverity.Warning)
        {
            return WarningIcon;
        }

        return MessageIcon;
    }

    void ToggleShowSource(LogStackFrame frame)
    {
        ShowFrameSource = !ShowFrameSource;
    }

    void JumpToSource(LogStackFrame frame)
    {
        var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), frame.FileName);
        if (System.IO.File.Exists(filename))
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(frame.FileName, frame.LineNumber);
        }
    }

    void DrawFrameSource(LogStackFrame frame)
    {
        var style = new GUIStyle(GUI.skin.textArea);
        style.richText = true;
        var source = GetSourceForFrame(frame);
        if(!String.IsNullOrEmpty(source))
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label(source, style);
            EditorGUILayout.EndVertical();
        }
    }

    void DrawFilter()
    {
        EditorGUILayout.BeginHorizontal();
        LabelClamped("Filter Regex", GUI.skin.label);
        var filterRegex = EditorGUILayout.TextArea(FilterRegex);
        if(ButtonClamped("Clear", GUI.skin.button))
        {
            filterRegex = null;
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
        }
        //If the filter has changed, invalidate our currently selected message
        if(filterRegex!=FilterRegex)
        {
            ClearSelectedMessage();
            FilterRegex = filterRegex;
        }
            
        EditorGUILayout.EndHorizontal();
    }

    List<string> GetChannels()
    {
        var categories = EditorLogger.Channels;
        
        var channelList = new List<string>();
        channelList.Add("All");
        channelList.Add("No Channel");
        channelList.AddRange(categories);
        return channelList;
    }

    /// <summary>
    ///   Handles the split window stuff, somewhat bodgily
    /// </summary>
    private void ResizeTopPane()
    {
        //Set up the resize collision rect
        CursorChangeRect = new Rect(0, CurrentTopPaneHeight, position.width, 5f);

        var oldColor = GUI.color;
        GUI.color = SizerLineColour; 
        GUI.DrawTexture(CursorChangeRect,EditorGUIUtility.whiteTexture);
        GUI.color = oldColor;
        EditorGUIUtility.AddCursorRect(CursorChangeRect,MouseCursor.ResizeVertical);
         
        if( Event.current.type == EventType.mouseDown && CursorChangeRect.Contains(Event.current.mousePosition))
        {
            Resize = true;
        }
        
        if(Resize)
        {
            CurrentTopPaneHeight = Event.current.mousePosition.y;
            CursorChangeRect.Set(CursorChangeRect.x,CurrentTopPaneHeight,CursorChangeRect.width,CursorChangeRect.height);
        }

        if(Event.current.type == EventType.MouseUp)
            Resize = false;

        CurrentTopPaneHeight = Mathf.Clamp(CurrentTopPaneHeight, 100, position.height-100);
    }

    //Cache for GetSourceForFrame
    string SourceLines;
    LogStackFrame SourceLinesFrame;

    /// <summary>
    ///If the frame has a valid filename, get the source string for the code around the frame
    ///This is cached, so we don't keep getting it.
    /// </summary>
    string GetSourceForFrame(LogStackFrame frame)
    {
        if(SourceLinesFrame==frame)
        {
            return SourceLines;
        }
        

        if(frame.FileName==null)
        {
            return "";
        }
        var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), frame.FileName);
        if (!System.IO.File.Exists(filename))
        {
            return "";
        }

        int lineNumber = frame.LineNumber-1;
        int linesAround = 3;
        var lines = System.IO.File.ReadAllLines(filename);
        var firstLine = Mathf.Max(lineNumber-linesAround, 0);
        var lastLine = Mathf.Min(lineNumber+linesAround+1, lines.Count());

        SourceLines = "";
        if(firstLine!=0)
        {
            SourceLines += "...\n";
        }
        for(int c1=firstLine; c1<lastLine; c1++)
        {
            string str = lines[c1] + "\n";
            if(c1==lineNumber)
            {
                str = "<color=#ff0000ff>"+str+"</color>";
            }
            SourceLines += str;
        }
        if(lastLine!=lines.Count())
        {
            SourceLines += "...\n";
        }

        SourceLinesFrame = frame;
        return SourceLines;
    }

    void ClearSelectedMessage()
    {
        SelectedMessage = -1;
        SelectedCallstackFrame = -1;
        ShowFrameSource = false;
    }

    Vector2 LogListScrollPosition;
    Vector2 LogDetailsScrollPosition;

    Texture2D ErrorIcon;
    Texture2D WarningIcon;
    Texture2D MessageIcon;
    Texture2D SmallErrorIcon;
    Texture2D SmallWarningIcon;
    Texture2D SmallMessageIcon;

    bool ShowTimes = true;
    float CurrentTopPaneHeight;
    bool Resize = false;
    Rect CursorChangeRect;
    int SelectedMessage = -1;

    double LastMessageClickTime = 0;
    double LastFrameClickTime = 0;


    //Serialise the logger field so that Unity doesn't forget about the logger when you hit Play
    [UnityEngine.SerializeField]
    UberLoggerEditor EditorLogger;

    //Standard unity pro colours
    // Color SelectedLineColour = new Color(35.0f/255.0f, 95.0f/255.0f, 153.0f/255.0f);
    Color LineColour1;
    Color LineColour2;
    Color SizerLineColour;

    GUIStyle LogLineStyle;
    GUIStyle SelectedLogLineStyle;
    string CurrentChannel=null;
    string FilterRegex = null;
    bool ShowErrors = true; 
    bool ShowWarnings = true; 
    bool ShowMessages = true; 
    int SelectedCallstackFrame = 0;
    bool ShowFrameSource = false;
}
