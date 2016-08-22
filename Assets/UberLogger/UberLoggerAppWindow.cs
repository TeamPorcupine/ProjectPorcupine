using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UberLogger;


/// <summary>
/// The in-app console logging frontend and backend
/// </summary>
public class UberLoggerAppWindow : MonoBehaviour, UberLogger.ILogger
{
    public GUISkin Skin;
    public Texture2D SmallErrorIcon;
    public Texture2D SmallWarningIcon;
    public Texture2D SmallMessageIcon;
    public Color GUIColour = new Color(1, 1, 1, 0.5f);

    //If non-zero, scales the fonts
    public int FontSize = 0;
    public Color SizerLineColour = new Color(42.0f/255.0f, 42.0f/255.0f, 42.0f/255.0f);
    public float SizerStartHeightRatio = 0.75f;

    public void Log(LogInfo logInfo)
    {
        LogInfo.Add(logInfo);
        if(logInfo.Severity==LogSeverity.Error)
        {
            NoErrors++;
        }
        else if(logInfo.Severity==LogSeverity.Warning)
        {
            NoWarnings++;
        }
        else
        {
            NoMessages++;
        }
        if(logInfo.Severity==LogSeverity.Error && PauseOnError)
        {
            UnityEngine.Debug.Break();
        }
    }

    void Clear()
    {
        LogInfo.Clear();
        NoWarnings = 0;
        NoErrors = 0;
        NoMessages = 0;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        UberLogger.Logger.AddLogger(this);
        ClearSelectedMessage();
        WindowRect = new Rect(0,0, Screen.width/2, Screen.height);
        CurrentTopPaneHeight = Screen.height*SizerStartHeightRatio;
    }

    bool ShowWindow = false;
    public Texture2D ButtonTexture;
    public Texture2D ErrorButtonTexture;
    public Vector2 ButtonPosition;
    public Vector2 ButtonSize = new Vector2(32, 32);
    
    /// <summary>
    ///   Shows either the activation button or the full UI
    /// </summary>
    public void OnGUI()
    {
        GUI.skin = Skin;
        if(ShowWindow)
        {
            var oldGUIColor = GUI.color;
            GUI.color = GUIColour;
            WindowRect = new Rect(0,0, Screen.width/2, Screen.height);
            //Set up the basic style, based on the Unity defaults
            LogLineStyle1 = Skin.customStyles[0];
            LogLineStyle2 = Skin.customStyles[1];
            SelectedLogLineStyle = Skin.customStyles[2];

        
            
            LogLineStyle1.fontSize = FontSize;
            LogLineStyle2.fontSize = FontSize;
            SelectedLogLineStyle.fontSize = FontSize;

            WindowRect = GUILayout.Window(1, WindowRect, DrawWindow, "Uber Console", GUI.skin.window);
            GUI.color = oldGUIColor;
        }
        else
        {
            DrawActivationButton();
        }
    }

    public void DrawActivationButton()
    {
        Texture2D buttonTex = ButtonTexture;
        if(NoErrors>0)
        {
            buttonTex = ErrorButtonTexture;
        }
        var buttonPos = ButtonPosition;
        buttonPos.x*=Screen.width;
        buttonPos.y*=Screen.height;
        if(buttonPos.x+ButtonSize.x> Screen.width)
        {
            buttonPos.x = Screen.width-ButtonSize.x;
        }
        if(buttonPos.y+ButtonSize.y > Screen.height)
        {
            buttonPos.y = Screen.height-ButtonSize.y;
        }
        var buttonRect = new Rect(buttonPos.x, buttonPos.y, ButtonSize.x, ButtonSize.y);
        var style = new GUIStyle();

        if(GUI.Button(buttonRect, buttonTex, style))
        {
            ShowWindow = !ShowWindow;
        }
    }

    /// <summary>
    /// Draws the main window
    /// </summary>
    void DrawWindow(int windowID)
    {
        // GUI.DragWindow(new Rect(0, 0, 10000, 20));
        var oldGUIColour = GUI.color;
        GUI.color = GUIColour;
        GUILayout.BeginVertical(GUILayout.Height(CurrentTopPaneHeight-GUI.skin.window.padding.top), GUILayout.MinHeight(100));
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
        GUI.color = oldGUIColour;

        DrawActivationButton();
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
        var toolbarStyle = GUI.skin.customStyles[3];
        GUILayout.BeginHorizontal();
        if(ButtonClamped("Clear", toolbarStyle))
        {
            Clear();
        }
        // PauseOnError  = ToggleClamped(PauseOnError, "Pause On Error", toolbarStyle);
        ShowTimes = ToggleClamped(ShowTimes, "Show Times", toolbarStyle);

        var buttonSize = toolbarStyle.CalcSize(new GUIContent("T")).y;
        GUILayout.FlexibleSpace();

        var showErrors = ToggleClamped(ShowErrors, new GUIContent(NoErrors.ToString(), SmallErrorIcon), toolbarStyle, GUILayout.Height(buttonSize));
        var showWarnings = ToggleClamped(ShowWarnings, new GUIContent(NoWarnings.ToString(), SmallWarningIcon), toolbarStyle, GUILayout.Height(buttonSize));
        var showMessages = ToggleClamped(ShowMessages, new GUIContent(NoMessages.ToString(), SmallMessageIcon), toolbarStyle, GUILayout.Height(buttonSize));
        //If the errors/warning to show has changed, clear the selected message
        if(showErrors!=ShowErrors || showWarnings!=ShowWarnings || showMessages!=ShowMessages)
        {
            ClearSelectedMessage();
        }
        ShowWarnings = showWarnings;
        ShowMessages = showMessages;
        ShowErrors = showErrors;
        GUILayout.EndHorizontal();
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

        LogListScrollPosition = GUILayout.BeginScrollView(LogListScrollPosition);
        var maxLogPanelHeight = WindowRect.height;
                
        float buttonY = 0;
        float buttonHeight = LogLineStyle1.CalcSize(new GUIContent("Test")).y;
        
        System.Text.RegularExpressions.Regex filterRegex = null;

        if(!String.IsNullOrEmpty(FilterRegex))
        {
            filterRegex = new System.Text.RegularExpressions.Regex(FilterRegex);
        }

        int drawnButtons = 0;
        var logLineStyle = LogLineStyle1;
        for(int c1=0; c1<LogInfo.Count; c1++)
        {
            var log = LogInfo[c1];
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
                    }
                    else
                    {
                        logLineStyle = (drawnButtons%2==0) ? LogLineStyle1 : LogLineStyle2;
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
                            if(Time.realtimeSinceStartup-LastMessageClickTime<0.3f)
                            {
                                LastMessageClickTime = 0;
                            }
                            else
                            {
                                LastMessageClickTime = Time.realtimeSinceStartup;
                            }
                        }
                        else
                        {
                            SelectedMessage = c1;
                            SelectedCallstackFrame = -1;
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
        GUILayout.EndScrollView();
        GUI.backgroundColor = oldColor;
    }


    /// <summary>
    /// The bottom of the panel - details of the selected log
    /// </summary>
    public void DrawLogDetails()
    {
        var oldColor = GUI.backgroundColor;
        SelectedMessage = Mathf.Clamp(SelectedMessage, 0, LogInfo.Count);
        if(LogInfo.Count>0 && SelectedMessage>=0)
        {
            LogDetailsScrollPosition = GUILayout.BeginScrollView(LogDetailsScrollPosition);
            var log = LogInfo[SelectedMessage];
            var logLineStyle = LogLineStyle1;
            for(int c1=0; c1<log.Callstack.Count; c1++)
            {
                var frame = log.Callstack[c1];
                var methodName = frame.GetFormattedMethodName();
                if(!String.IsNullOrEmpty(methodName))
                {
                    if(c1==SelectedCallstackFrame)
                    {
                        // GUI.backgroundColor = Color.white;
                        logLineStyle = SelectedLogLineStyle;
                    }
                    else
                    {
                        logLineStyle = (c1%2==0) ? LogLineStyle1 : LogLineStyle2;
                    }
                    

                    if(GUILayout.Button(methodName, logLineStyle))
                    {
                        SelectedCallstackFrame = c1;
                    }
                }

            }
            GUILayout.EndScrollView();
        }
        GUI.backgroundColor = oldColor;
    }

    Texture2D GetIconForLog(LogInfo log)
    {
        if(log.Severity==LogSeverity.Error)
        {
            return SmallErrorIcon;
        }
        if(log.Severity==LogSeverity.Warning)
        {
            return SmallWarningIcon;
        }

        return SmallMessageIcon;
    }

    void DrawFilter()
    {
        GUILayout.BeginHorizontal();
        LabelClamped("Filter Regex", GUI.skin.label);
        var filterRegex = GUILayout.TextArea(FilterRegex);
        if(ButtonClamped("Clear", GUI.skin.button))
        {
            filterRegex = "";
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
        }
        //If the filter has changed, invalidate our currently selected message
        if(filterRegex!=FilterRegex)
        {
            ClearSelectedMessage();
            FilterRegex = filterRegex;
        }
            
        GUILayout.EndHorizontal();
    }

    List<string> GetChannels()
    {
        var categories = new HashSet<string>();
        foreach(var logInfo in LogInfo)
        {
            if(!String.IsNullOrEmpty(logInfo.Channel) && !categories.Contains(logInfo.Channel))
            {
                categories.Add(logInfo.Channel);
            }
        }
        
        var channelList = new List<string>();
        channelList.Add("All");
        channelList.Add("No Channel");
        channelList.AddRange(categories);
        return channelList;
    }

    bool Resizing = false;
    private void ResizeTopPane()
    {
        //Set up the resize collision rect
        // float offset = GUI.skin.window.border.bottom;
        float offset = 0;
        var resizerRect = new Rect(0, CurrentTopPaneHeight+offset, WindowRect.width, 5f);

        var oldColor = GUI.color;
        GUI.color = SizerLineColour; 
        GUI.DrawTexture(resizerRect, Texture2D.whiteTexture);
        GUI.color = oldColor;

        if( Event.current.type == EventType.mouseDown && resizerRect.Contains(Event.current.mousePosition))
        {
            Resizing = true;
        }

        if(Resizing)
        {
            CurrentTopPaneHeight = Event.current.mousePosition.y;
        }

        if(Event.current.type == EventType.MouseUp)
        {
            Resizing = false;
        }
        

        CurrentTopPaneHeight = Mathf.Clamp(CurrentTopPaneHeight, 100, WindowRect.height-100);
    }

    void ClearSelectedMessage()
    {
        SelectedMessage = -1;
        SelectedCallstackFrame = -1;
    }

    Vector2 LogListScrollPosition;
    Vector2 LogDetailsScrollPosition;


    bool ShowTimes = true;
    float CurrentTopPaneHeight;
    int SelectedMessage = -1;

    double LastMessageClickTime = 0;

    List<UberLogger.LogInfo> LogInfo = new List<LogInfo>();
    bool PauseOnError = false;
    int NoErrors;
    int NoWarnings;
    int NoMessages;
    Rect WindowRect = new Rect(0,0, 100, 100);


    GUIStyle LogLineStyle1;
    GUIStyle LogLineStyle2;
    GUIStyle SelectedLogLineStyle;
    string CurrentChannel=null;
    string FilterRegex = "";
    
    bool ShowErrors = true; 
    bool ShowWarnings = true; 
    bool ShowMessages = true; 
    int SelectedCallstackFrame = 0;
}
