using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DeveloperConsole.CommandTypes;
using System.Linq;

public class DevConsole : MonoBehaviour
{
    [SerializeField]
    bool dontDestroyOnLoad;                                         // Destroy on Load?
    [SerializeField]
    KeyCode consoleActivationKey = KeyCode.Backslash;               // Open/Close console
    [Range(8, 20)]
    public int fontSize = 15;

    const int Autoclear_Threshold = 18000;                          // Charactres to do a Clear

    public string inputText = "";

    List<CommandBase> consoleCommands = new List<CommandBase>();                              // Whole list of commands available

    // Flags
    bool closed = true;
    bool opened
    {
        get
        {
            return !closed;
        }
        set
        {
            closed = !value;
        }
    }

    bool showTimeStamp = false;

    // AutoComplete
    List<string> possibleCandidates = new List<string>();           // Possible options that the user is trying to type
    int selectedCandidate = 0;                                      // Index of the option chosen

    static DevConsole instance;                                     // To prevent multiple

    // Connections to UI
    [SerializeField]
    Text textArea;
    [SerializeField]
    InputField inputField;
    [SerializeField]
    GameObject autoComplete;
    [SerializeField]
    ScrollRect scrollRect;

    void Awake()
    {
        if (instance == null || instance == this)
        {
            instance = this;
        }
        else
        {
            Debug.ULogWarningChannel("DevConsole", "There can only be one Console per project");
            Destroy(this);
        }

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        if (textArea == null || inputField == null || autoComplete == null || scrollRect == null)
        {
            gameObject.SetActive(false);
            Debug.ULogError("DevConsole", "Missing gameobjects, look at the serializeable fields");
        }

        textArea.fontSize = fontSize;

        LoadCommands();
    }

    void Update()
    {
        if (Input.GetKeyUp(consoleActivationKey))
        {
            Open();
        }
    }

    void LoadCommands()
    {
        // Load Base Commands
        AddCommands(
            new Command("Clear", Clear, "Clears the console"),
            new Command<bool>("ShowTimeStamp", ShowTimeStamp, "Establishes whether or not to show the time stamp for each command"),
            new Command("Help", Help, "Shows a list of all the available commands"),
            new Command<int>("SetFontSize", SetFontSize, "Sets the font size of the console"),
            new Command("quit", Application.Quit, "Quits the game")
            );

        // Load ProjectPorcupine specific commands


        // Load Commands from C#
        // Empty because C# Modding not implemented yet
        // TODO: Once C# Modding Implemented, add the ability to add commands here

        // Load Commands from JSON



    }

    public static void Open()
    {
        if (instance == null || instance.opened)
        {
            return;
        }

        instance.opened = true;
        instance.gameObject.SetActive(true);
    }

    public static void Close()
    {
        if (instance == null || instance.closed)
        {
            return;
        }

        instance.closed = true;
        instance.inputText = "";
        instance.gameObject.SetActive(false);
    }

    public void EnterPressedForInput(Text newValue)
    {
        // Clear input but retrieve
        inputText = newValue.text;
        inputField.text = "";

        // Add Text
        Log(inputText);

        // Execute
        Execute(inputText);
    }

    public void RegenerateAutoComplete(Text newValue)
    {
        inputText = newValue.text;

        if (inputText == "" || consoleCommands == null || consoleCommands.Count == 0)
        {
            return;
        }

        string candidateTester = "";

        if (possibleCandidates.Count != 0 && selectedCandidate >= 0 && possibleCandidates.Count > selectedCandidate)
        {
            // Set our candidates tester because we had a candidate before and we want it to carry over
            candidateTester = possibleCandidates[selectedCandidate];
        }

        List<string> possible = consoleCommands
            .Where(cB => cB.title.ToLower().StartsWith(inputText.ToLower()))
            .Select(cB => cB.title).ToList();

        if (possible != null)
        {
            possibleCandidates = possible;
        }

        if (candidateTester == "")
        {
            selectedCandidate = 0;
            return;
        }

        for (int i = 0; i < possibleCandidates.Count; i++)
        {
            if (possibleCandidates[i] == candidateTester)
            {
                selectedCandidate = i;
                return;
            }
        }
        selectedCandidate = 0;
    }

    #region LogManagement

    /// <summary>
    /// Logs to the console
    /// </summary>
    /// <param name="obj"> Must be convertable to string </param>
    public static void Log(object obj)
    {
        if (instance == null)
        {
            return;
        }

        BasePrint(obj.ToString());
    }

    /// <summary>
    /// Logs to the console
    /// </summary>
    /// <param name="obj"> Must be convertable to string </param>
    /// <param name="color"> Can either be the name of one the common names or can be in HTML format </param>
    public static void Log(object obj, string color)
    {
        if (instance == null)
        {
            return;
        }

        BasePrint("<color=" + color + ">" + obj.ToString() + "</color>");
    }

    /// <summary>
    /// Logs to the console with color yellow
    /// </summary>
    /// <param name="obj"> Must be convertable to string </param>
    public static void LogWarning(object obj)
    {
        if (instance == null)
        {
            return;
        }

        BasePrint("<color=yellow>" + obj.ToString() + "</color>");
    }

    /// <summary>
    /// Logs to the console with color red
    /// </summary>
    /// <param name="obj"> Must be convertable to string </param>
    public static void LogError(object obj)
    {
        if (instance == null)
        {
            return;
        }

        BasePrint("<color=red>" + obj.ToString() + "</color>");
    }

    /// <summary>
    /// Logs to the console
    /// </summary>
    /// <param name="text"> Text to print </param>
    public static void BasePrint(string text)
    {
        if (instance == null)
        {
            return;
        }

        instance.textArea.text += "\n" + text + ((instance.showTimeStamp == true) ? "\t[" + System.DateTime.Now.ToShortTimeString() + "]" : "");

        // Clear if limit exceeded
        if (instance.textArea.text.Length >= Autoclear_Threshold)
        {
            instance.textArea.text = "AUTO-CLEAR";
        }

        print("HERE");
        // Update scroll bar
        Canvas.ForceUpdateCanvases();
        instance.scrollRect.verticalNormalizedPosition = 0;
        Canvas.ForceUpdateCanvases();
    }

    #endregion

    #region CommandManagement

    public static void Execute(string command)
    {
        // Guard
        if (instance == null)
        {
            return;
        }

        // Get method and arguments
        string[] methodNameAndArgs = command.Split(':');
        string method = command;
        string args = "";

        IEnumerable<CommandBase> commandsToCall;

        if (methodNameAndArgs.Length == 2)
        {
            // If both method and arguments exist split them
            method = methodNameAndArgs[0];
            args = methodNameAndArgs[1];
        }
        else if (method.Trim().EndsWith("?"))
        {
            // This is help
            string testString = method.ToLower().Trim().TrimEnd('?');
            commandsToCall = instance.consoleCommands.Where(cB => cB.title.ToLower() == testString);

            foreach (CommandBase commandToCall in commandsToCall)
            {
                commandToCall.ShowHelp();
            }

            return;
        }

        // Execute command
        commandsToCall = instance.consoleCommands.Where(cB => cB.title.ToLower() == method.ToLower().Trim());

        foreach (CommandBase commandToCall in commandsToCall)
        {
            commandToCall.ExecuteCommand(args);
        }
    }

    public static void AddCommands(params CommandBase[] commands)
    {
        // Guard
        if (instance == null)
        {
            return;
        }

        instance.consoleCommands.AddRange(commands);
    }

    public static void AddCommand(CommandBase command)
    {
        // Guard
        if (instance == null)
        {
            return;
        }

        instance.consoleCommands.Add(command);
    }

    public static bool CommandExists(string commandTitle)
    {
        // Guard
        if (instance == null)
        {
            return false;
        }

        // Could just stop at first, but its a lazy search so its pretty darn fast anyways
        return (instance.consoleCommands.Count(cB => cB.title.ToLower() == commandTitle.ToLower()) > 0) ? true : false;
    }

    public static void RemoveCommand(string commandTitle)
    {
        // Guard
        if (instance == null)
        {
            return;
        }

        int oldCount = instance.consoleCommands.Count;
        // Safe delete
        instance.consoleCommands.RemoveAll(cB => cB.title.ToLower() == commandTitle.ToLower());

        Log("Deleted " + (instance.consoleCommands.Count - oldCount).ToString() + " commands");
    }

    #endregion

    #region BaseCommands

    void Help()
    {
        string text = "";
        for (int i = 0; i < consoleCommands.Count; i++)
        {
            text += "\n<color=orange>" + consoleCommands[i].title + "</color>" + (consoleCommands[i].helpText == null ? "" : ": " + consoleCommands[i].helpText);
        }
        Log("-- Help --" + text);
    }

    void Clear()
    {
        if (instance == null)
        {
            return;
        }

        instance.textArea.text = "";
        Log("Clear Successful :D", "green");
    }

    void ShowTimeStamp(bool value)
    {
        print(value);
        showTimeStamp = value;
        Log("Change successful :D", "green");
    }

    void SetFontSize(int size)
    {
        textArea.fontSize = size;
        Log("Change successful :D", "green");
    }

    #endregion
}
