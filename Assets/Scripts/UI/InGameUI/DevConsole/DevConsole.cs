﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DeveloperConsole.CommandTypes;
using DeveloperConsole.Interfaces;
using System.Linq;
using MoonSharp.Interpreter;

namespace DeveloperConsole
{
    [MoonSharpUserData]
    public class DevConsole : MonoBehaviour
    {
        [Range(8, 20)]
        public int fontSize = 15;                                                            // Starting font size (only changes the font on start)

        const int Autoclear_Threshold = 18000;                                               // Max characters before cleaning

        string inputText = "";                                                               // The current text

        List<CommandBase> consoleCommands = new List<CommandBase>();         // Whole list of commands available

        List<string> history = new List<string>();                                           // History of inputs
        int currentHistoryIndex = -1;                                                        // What index the history is at

        // Flags
        bool closed = true;                                                                  // Is the command line closed
        bool opened                                                                          // Is the command line open
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

        bool showTimeStamp = false;                                                          // Display a time stamp at the end of every command

        [SerializeField]
        /// <summary>
        /// What mode the console is:
        /// 0 is Function :x, y, z:
        /// 1 is Function (x, y, z)   
        /// 2 is Function {x, y, z}   
        /// 3 is Function [x, y, z]   
        /// 4 is Function &ltx, y, z&gt
        /// </summary>
        int developerCommandMode = 0;                                                        // The current mode that input is recieved as

        // AutoComplete
        List<string> possibleCandidates = new List<string>();                                // Possible options that the user is trying to type
        int selectedCandidate = 0;                                                           // Index of the option chosen

        /// <summary>
        /// Current instance
        /// </summary>
        static DevConsole instance;                                                          // To prevent multiple

        [SerializeField]
        Text textArea;                                                                       // The console text logger
        [SerializeField]
        InputField inputField;                                                               // The console input field
        [SerializeField]
        GameObject autoComplete;                                                             // The autocomplete object
        [SerializeField]
        ScrollRect scrollRect;                                                               // The scrolling rect for the main console logger
        [SerializeField]
        GameObject root;                                                                     // The root object to disable/enable for '\' or whatever consoleActivationKey is

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

            SceneManager.activeSceneChanged += SceneChanged;

            KeyboardManager.Instance.RegisterModalInputField(inputField);
            KeyboardManager.Instance.RegisterInputAction("DevConsole", KeyboardMappedInputType.KeyUp, ToggleConsole);
        }

        static void SceneChanged(Scene oldScene, Scene newScene)
        {
            if (instance != null)
            {
                KeyboardManager.Instance.UnRegisterModalInputField(instance.inputField);
                instance = null;
            }
        }

        // Use this for initialization
        void Start()
        {
            transform.SetAsLastSibling();

            if (textArea == null || inputField == null || autoComplete == null || scrollRect == null || root == null)
            {
                gameObject.SetActive(false);
                Debug.ULogError("DevConsole", "Missing gameobjects, look at the serializeable fields");
            }

            textArea.fontSize = fontSize;

            LoadCommands();

            if (root != null)
            {
                opened = root.activeInHierarchy;
            }
        }

        void ToggleConsole()
        {
            if (closed)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        void Update()
        {
            if (!inputField.isFocused)
            {
                currentHistoryIndex = -1;
                return;
            }

            // If input field is focused
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                if (currentHistoryIndex < (history.Count - 1)) // (history.Count -1) is the max index
                {
                    // This way the first index will be the last one so first in last out
                    // Kind of like a stack but you can go up and down it
                    currentHistoryIndex += 1;
                    inputField.text = history[(history.Count - 1) - (currentHistoryIndex)];
                }
                inputField.MoveTextEnd(false);
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                if (currentHistoryIndex > 0)
                {
                    // This way the first index will be the last one so first in last out
                    // Kind of like a stack but you can go up and down it
                    currentHistoryIndex -= 1;
                    inputField.text = history[history.Count - (currentHistoryIndex + 1)];
                }
                inputField.MoveTextEnd(false);
            }
        }

        void LoadCommands()
        {
            // Load Base Commands
            AddCommands(
                //new Command("Clear", Clear, "Clears the console"),
                //new Command<bool>("ShowTimeStamp", ShowTimeStamp, "Establishes whether or not to show the time stamp for each command"),
                //new Command("Help", Help, "Shows a list of all the available commands"),
                new VectorCommand<Vector3>("ChangeCameraCSharp", ChangeCameraPositionCSharp, "Change Camera Position (Written in CSharp)")
                //             new Command<int>("SetFontSize", SetFontSize, "Sets the font size of the console"),
                //              new Command("quit", Application.Quit, "Quits the game")
                );

            // Load ProjectPorcupine specific commands


            // Load Commands from C#
            // Empty because C# Modding not implemented yet
            // TODO: Once C# Modding Implemented, add the ability to add commands here

            // Load Commands from XML (will be changed to JSON AFTER the current upgrade)
            AddCommands(PrototypeManager.DevConsole.Values.Select(x => x.luaCommand).ToArray());
        }

        public static void Open()
        {
            if (instance == null || instance.opened)
            {
                return;
            }

            instance.opened = true;
            instance.root.SetActive(true);
        }

        public static void Close()
        {
            if (instance == null || instance.closed)
            {
                return;
            }

            instance.closed = true;
            instance.inputText = "";
            instance.root.SetActive(false);
        }

        public void EnterPressedForInput(Text newValue)
        {
            currentHistoryIndex = -1;
            // Clear input but retrieve
            inputText = newValue.text;
            inputField.text = "";

            if (inputText != "")
            {
                // Add Text to log
                Log(inputText);

                // Add to history
                history.Add(inputText);

                // Execute
                Execute(inputText);
            }
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
            string[] methodNameAndArgs;
            string method;
            string args;

            switch (instance.developerCommandMode)
            {
                case 1:
                    methodNameAndArgs = command.Trim().TrimEnd(')').Split('(');
                    method = command;
                    args = "";
                    break;
                case 2:
                    methodNameAndArgs = command.Trim().TrimEnd('}').Split('{');
                    method = command;
                    args = "";
                    break;
                case 3:
                    methodNameAndArgs = command.Trim().TrimEnd(']').Split('[');
                    method = command;
                    args = "";
                    break;
                case 4:
                    methodNameAndArgs = command.Trim().TrimEnd('>').Split('<');
                    method = command;
                    args = "";
                    break;

                case 0:
                default:
                    methodNameAndArgs = command.Trim().TrimEnd(':').Split(':');
                    method = command;
                    args = "";
                    break;
            }

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
                    if (commandToCall.helpMethod != null)
                    {
                        ShowHelpMethod(commandToCall);
                    }
                    else
                    {
                        ShowDescription(commandToCall);
                    }
                }

                return;
            }

            // Execute command
            commandsToCall = instance.consoleCommands.Where(cB => cB.title.ToLower() == method.ToLower().Trim());

            foreach (CommandBase commandToCall in commandsToCall)
            {
                ICommandRunnable runnable = (ICommandRunnable)commandToCall;

                if (runnable != null)
                {
                    runnable.ExecuteCommand(args);
                }
            }
        }

        public static void ShowHelpMethod(ICommandHelpMethod help)
        {
            if (help.helpMethod != null)
            {
                help.helpMethod();
            }
        }

        public static void ShowDescription(CommandBase description)
        {
            Log("<color=yellow>Command Info:</color> " + ((description.descriptiveText == "") ? "<color=red>There's no help for this command</color>" : description.descriptiveText));
            Log("<color=yellow>Call: <color=orange>" + description.title + GetParametersWithConsoleMode(description) + "</color>");
        }

        public static void AddCommands(IEnumerable<CommandBase> commands)
        {
            // Guard
            if (instance == null)
            {
                return;
            }

            instance.consoleCommands.AddRange(commands);
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

        public static IEnumerator<CommandBase> CommandIterator()
        {
            return (instance != null) ? instance.consoleCommands.GetEnumerator() : new List<CommandBase>.Enumerator();
        }

        public static CommandBase[] CommandArray()
        {
            return (instance != null) ? instance.consoleCommands.ToArray() : new CommandBase[] { };
        }

        public static void AddHistory(string command)
        {
            if (instance == null)
            {
                return;
            }

            instance.history.Add(command);
        }

        public static void RemoveHistory(string command)
        {
            if (instance == null || instance.history.Contains(command) == false)
            {
                return;
            }

            instance.history.Remove(command);
        }

        public static void ClearHistory()
        {
            if (instance == null)
            {
                return;
            }

            instance.history.Clear();
        }

        #endregion

        #region BaseCommands

        public static void ChangeCameraPositionCSharp(Vector3 newPos)
        {
            Camera.main.transform.position = newPos;
        }

        public void Help()
        {
            string text = "";

            for (int i = 0; i < consoleCommands.Count; i++)
            {
                text += "\n<color=orange>" + consoleCommands[i].title + GetParametersWithConsoleMode(consoleCommands[i]) + "</color>" + (consoleCommands[i].descriptiveText == null ? "" : " //" + consoleCommands[i].descriptiveText);
            }

            text += "\n<color=orange>Note:</color> If the function has no parameters you <color=red>don't</color> need to use the parameter modifier.";
            text += "\n<color=orange>Note:</color> You <color=red>don't</color> need to use the trailing parameter modifier either";

            Log("-- Help --" + text);
        }

        public static int DeveloperCommandMode()
        {
            return (instance == null) ? -1 : instance.developerCommandMode;
        }

        public static string GetParametersWithConsoleMode(CommandBase description)
        {
            if (instance == null)
            {
                return "";
            }

            switch (instance.developerCommandMode)
            {
                case 1:
                    return " (" + description.parameters + ")";
                case 2:
                    return " {" + description.parameters + "}";
                case 3:
                    return " [" + description.parameters + "]";
                case 4:
                    return " <" + description.parameters + ">";
                case 0:
                default:
                    return " : " + description.parameters + " :";
            }
        }

        public static void Clear()
        {
            if (instance == null)
            {
                return;
            }

            instance.textArea.text = "";
            Log("Clear Successful :D", "green");
        }

        public static void ShowTimeStamp(bool value)
        {
            if (instance == null)
            {
                return;
            }

            instance.showTimeStamp = value;
            Log("Change successful :D", "green");
        }

        public static void SetText(string text)
        {
            if (instance == null)
            {
                return;
            }

            instance.textArea.text = text;
        }

        public static void SetTextSize(int size)
        {
            if (instance == null)
            {
                return;
            }

            instance.textArea.fontSize = size;
        }

        public static Text TextObject()
        {
            if (instance == null)
            {
                return null;
            }

            return instance.textArea;
        }

        void SetFontSize(int size)
        {
            if (size < 10)
            {
                LogError("Font size would be too small");
            }
            else if (size > 20)
            {
                LogError("Font size would be too big");
            }
            else
            {
                textArea.fontSize = size;
                Log("Change successful :D", "green");
            }
        }

        #endregion
    }
}
