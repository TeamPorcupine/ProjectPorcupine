using UnityEngine;
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

    public int fontSize = 15;

    const int Autoclear_Threshold = 18000;                          // Charactres to do a Clear

    List<CommandBase> consoleCommands;                              // Whole list of commands available

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
    bool showHelp = true;
    bool showTimeStamp = false;

    // AutoComplete
    List<string> possibleCandidates = new List<string>();           // Possible options that the user is trying to type
    int selectedCandidate = 0;                                      // Index of the option chosen

    // Buffer
    List<KeyValuePair<string, string>> buffer =
        new List<KeyValuePair<string, string>>();                   // Messages buffer.  Stored to print out next time we are to update UI

    static DevConsole instance;                                    // To prevent multiple

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

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadCommands()
    {

    }

    public static void Open()
    {
        if (instance == null || instance.opened)
        {
            return;
        }

        instance.opened = true;
    }

    public static void Close()
    {
        if (instance == null || instance.closed)
        {
            return;
        }

        // Startin animation;
        instance.closed = true;
    }

    void AutoCompleteBasedOnCandidates()
    {
        string candidateTester = "";

        if (possibleCandidates.Count != 0 && selectedCandidate >= 0 && possibleCandidates.Count > selectedCandidate)
        {
            // Set our candidates tester because we had a candidate before and we want it to carry over
            candidateTester = possibleCandidates[selectedCandidate];
        }

        for (int i = 0; i < consoleCommands.Count; i++)
        {

        }
    }

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

        // LogInfo("Deleted " + instance.consoleCommands.Count - oldCount + " commands")
    }

    #endregion
}
