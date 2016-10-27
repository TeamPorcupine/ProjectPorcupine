#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using DeveloperConsole.CommandTypes;
using DeveloperConsole.Interfaces;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeveloperConsole
{
    public delegate void HelpMethod();

    [MoonSharpUserData]
    public class DevConsole : MonoBehaviour
    {
        /// <summary>
        /// Max characters before cleaning.
        /// </summary>
        private const int AutoclearThreshold = 18000;

        /// <summary>
        /// Current instance.
        /// </summary>
        private static DevConsole instance;

        /// <summary>
        /// The last inputed text.
        /// </summary>
        private string inputText = string.Empty;

        /// <summary>
        /// Whole list of commands available.
        /// </summary>
        private List<CommandBase> consoleCommands = new List<CommandBase>();

        /// <summary>
        /// History of commands.
        /// </summary>
        private List<string> history = new List<string>();

        /// <summary>
        /// What index the history is at.
        /// </summary>
        private int currentHistoryIndex = -1;

        /// <summary>
        /// Is the command console closed.
        /// </summary>
        private bool closed = true;

        /// <summary>
        /// Possible options that the user could be trying to type.
        /// </summary>
        private List<string> possibleCandidates = new List<string>();

        /// <summary>
        /// Index of the option chosen.
        /// </summary>
        private int selectedCandidate = -1;

        /// <summary>
        /// The console text logger.
        /// </summary>
        [SerializeField]
        private Text textArea;

        /// <summary>
        /// The input field for console commands.
        /// </summary>
        [SerializeField]
        private InputField inputField;

        /// <summary>
        /// The autocomplete object.
        /// </summary>
        [SerializeField]
        private ScrollRect autoComplete;

        /// <summary>
        /// Holds possible candidates.
        /// </summary>
        [SerializeField]
        private GameObject contentAutoComplete;

        /// <summary>
        /// The scrolling rect for the main console logger.
        /// </summary>
        [SerializeField]
        private ScrollRect scrollRect;

        /// <summary>
        /// The root object to disable/enable for '\' or whatever consoleActivationKey is.
        /// </summary>
        [SerializeField]
        private GameObject root;

        /// <summary>
        /// Is the command line open.
        /// </summary>
        private bool Opened
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

        /// <summary>
        /// Is autocomplete currently showing.
        /// </summary>
        private bool ShowingAutoComplete
        {
            get
            {
                return autoComplete.gameObject.activeInHierarchy;
            }
        }

        /// <summary>
        /// Opens the console.
        /// </summary>
        public static void Open()
        {
            if (instance == null || instance.Opened)
            {
                return;
            }

            if (CommandSettings.DeveloperConsoleToggle == false)
            {
                Close();
                return;
            }

            instance.Opened = true;
            instance.root.SetActive(true);
        }

        /// <summary>
        /// Closes the console.
        /// </summary>
        public static void Close()
        {
            if (instance == null || instance.closed)
            {
                return;
            }

            instance.closed = true;
            instance.inputText = string.Empty;
            instance.root.SetActive(false);
        }

        #region LogManagement

        /// <summary>
        /// Logs to the console.
        /// </summary>
        /// <param name="obj"> Must be convertable to string.</param>
        public static void Log(object obj)
        {
            if (instance == null)
            {
                return;
            }

            BasePrint(obj.ToString());
        }

        /// <summary>
        /// Logs to the console.
        /// </summary>
        /// <param name="obj"> Must be convertable to string.</param>
        /// <param name="color"> Can either be the name of one the common names or can be in HTML format.</param>
        public static void Log(object obj, string color)
        {
            if (instance == null)
            {
                return;
            }

            BasePrint("<color=" + color + ">" + obj.ToString() + "</color>");
        }

        /// <summary>
        /// Logs to the console with color yellow.
        /// </summary>
        /// <param name="obj"> Must be convertable to string.</param>
        public static void LogWarning(object obj)
        {
            if (instance == null)
            {
                return;
            }

            BasePrint("<color=yellow>" + obj.ToString() + "</color>");
        }

        /// <summary>
        /// Logs to the console with color red.
        /// </summary>
        /// <param name="obj"> Must be convertable to string.</param>
        public static void LogError(object obj)
        {
            if (instance == null)
            {
                return;
            }

            BasePrint("<color=red>" + obj.ToString() + "</color>");
        }

        /// <summary>
        /// Logs to the console.
        /// </summary>
        /// <param name="text"> Text to print.</param>
        public static void BasePrint(string text)
        {
            if (instance == null)
            {
                return;
            }

            instance.textArea.text += text + (CommandSettings.ShowTimeStamp ? "\t[" + System.DateTime.Now.ToShortTimeString() + "]" : string.Empty) + "\n";

            // Clear if limit exceeded
            if (instance.textArea.text.Length >= AutoclearThreshold)
            {
                instance.textArea.text = "\nAUTO-CLEAR";
            }

            // Update scroll bar
            Canvas.ForceUpdateCanvases();
            instance.scrollRect.verticalNormalizedPosition = 0;
            Canvas.ForceUpdateCanvases();
        }

        #endregion

        /// <summary>
        /// Executes the command passed.
        /// </summary>
        public static void Execute(string command)
        {
            // Guard
            if (instance == null)
            {
                return;
            }

            // Get method and arguments
            string[] methodNameAndArgs;
            string method = command;
            string args = string.Empty;

            switch (CommandSettings.DeveloperCommandMode)
            {
                case 1:
                    methodNameAndArgs = command.Trim().TrimEnd(')').Split('(');
                    break;
                case 2:
                    methodNameAndArgs = command.Trim().TrimEnd('}').Split('{');
                    break;
                case 3:
                    methodNameAndArgs = command.Trim().TrimEnd(']').Split('[');
                    break;
                case 4:
                    methodNameAndArgs = command.Trim().TrimEnd('>').Split('<');
                    break;

                case 0:
                default:
                    methodNameAndArgs = command.Trim().TrimEnd(':').Split(':');
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
                commandsToCall = instance.consoleCommands.Where(cB => cB.Title.ToLower() == testString);

                foreach (CommandBase commandToCall in commandsToCall)
                {
                    if (commandToCall.HelpMethod != null)
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
            commandsToCall = instance.consoleCommands.Where(cB => cB.Title.ToLower() == method.ToLower().Trim());

            foreach (CommandBase commandToCall in commandsToCall)
            {
                ICommandRunnable runnable = (ICommandRunnable)commandToCall;

                if (runnable != null)
                {
                    runnable.ExecuteCommand(args);
                }
            }
        }

        /// <summary>
        /// Runs the help method of the passed interface.
        /// </summary>
        public static void ShowHelpMethod(CommandBase help)
        {
            if (help.HelpMethod != null)
            {
                help.HelpMethod();
                Log("<color=yellow>Call it like </color><color=orange> " + help.Title + GetParametersWithConsoleMode(help) + "</color>");
            }
            else
            {
                ShowDescription(help);
            }
        }

        /// <summary>
        /// Logs the description of the passed command.
        /// </summary>
        public static void ShowDescription(CommandBase description)
        {
            Log("<color=yellow>Command Info:</color> " + ((description.DescriptiveText == string.Empty) ? " < color=red>There's no help for this command</color>" : description.DescriptiveText));
            Log("<color=yellow>Call it like </color><color=orange> " + description.Title + GetParametersWithConsoleMode(description) + "</color>");
        }

        /// <summary>
        /// Adds the given commands.
        /// </summary>
        public static void AddCommands(IEnumerable<CommandBase> commands)
        {
            // Guard
            if (instance == null)
            {
                return;
            }

            instance.consoleCommands.AddRange(commands);
        }

        /// <summary>
        /// Adds the given commands.
        /// </summary>
        public static void AddCommands(params CommandBase[] commands)
        {
            // Guard
            if (instance == null)
            {
                return;
            }

            instance.consoleCommands.AddRange(commands);
        }

        /// <summary>
        /// Adds the specified command.
        /// </summary>
        public static void AddCommand(CommandBase command)
        {
            // Guard
            if (instance == null)
            {
                return;
            }

            instance.consoleCommands.Add(command);
        }

        /// <summary>
        /// Returns true if the command exists.
        /// </summary>
        public static bool CommandExists(string commandTitle)
        {
            // Guard
            if (instance == null)
            {
                return false;
            }

            // Could just stop at first, but its a lazy search so its pretty darn fast anyways
            return (instance.consoleCommands.Count(cB => cB.Title.ToLower() == commandTitle.ToLower()) > 0) ? true : false;
        }

        /// <summary>
        /// Removes the commmand from the list of commands.
        /// </summary>
        public static void RemoveCommand(string commandTitle)
        {
            // Guard
            if (instance == null)
            {
                return;
            }

            int oldCount = instance.consoleCommands.Count;

            // Safe delete
            instance.consoleCommands.RemoveAll(cB => cB.Title.ToLower() == commandTitle.ToLower());

            Log("Deleted " + (instance.consoleCommands.Count - oldCount).ToString() + " commands");
        }

        /// <summary>
        /// Returns an IEnumerator that allows iteration consisting of all the commands.
        /// </summary>
        public static IEnumerator<CommandBase> CommandIterator()
        {
            return (instance != null) ? instance.consoleCommands.GetEnumerator() : new List<CommandBase>.Enumerator();
        }

        /// <summary>
        /// Returns an array of all the commands.
        /// </summary>
        public static CommandBase[] CommandArray()
        {
            return (instance != null) ? instance.consoleCommands.ToArray() : new CommandBase[] { };
        }

        /// <summary>
        /// Adds the specified command to the history.
        /// </summary>
        public static void AddHistory(string command)
        {
            if (instance == null)
            {
                return;
            }

            instance.history.Add(command);
        }

        /// <summary>
        /// Removes specified command from the history.
        /// </summary>
        public static void RemoveHistory(string command)
        {
            if (instance == null || instance.history.Contains(command) == false)
            {
                return;
            }

            instance.history.Remove(command);
        }

        /// <summary>
        /// Clears the command history.
        /// </summary>
        public static void ClearHistory()
        {
            if (instance == null)
            {
                return;
            }

            instance.history.Clear();
        }

        /// <summary>
        /// Changes the current camera position.
        /// </summary>
        public static void ChangeCameraPositionCSharp(Vector3 newPos)
        {
            Camera.main.transform.position = newPos;
        }

        /// <summary>
        /// Returns the current command mode.
        /// </summary>
        public static int DeveloperCommandMode()
        {
            return CommandSettings.DeveloperCommandMode;
        }

        /// <summary>
        /// Returns a parameter list using the correct command mode.
        /// </summary>
        /// <param name="description"> The description of the command.</param>
        public static string GetParametersWithConsoleMode(CommandBase description)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            switch (CommandSettings.DeveloperCommandMode)
            {
                case 1:
                    return " (" + description.Parameters + ")";
                case 2:
                    return " {" + description.Parameters + "}";
                case 3:
                    return " [" + description.Parameters + "]";
                case 4:
                    return " <" + description.Parameters + ">";
                case 0:
                default:
                    return " : " + description.Parameters + " :";
            }
        }

        /// <summary>
        /// Clears the text area and history.
        /// </summary>
        public static void Clear()
        {
            if (instance == null)
            {
                return;
            }

            instance.textArea.text = "\n";
            instance.history.Clear();
            Log("Clear Successful :D", "green");
        }

        /// <summary>
        /// Run the passed lua code.
        /// </summary>
        /// <param name="luaCode"> The LUA Code to run.</param>
        /// <remarks> 
        /// The code isn't vastly optimised since it should'nt be used for any large thing, 
        /// just to run a single command.
        /// </remarks>
        public static void Run_LUA(string luaCode)
        {
            new LuaFunctions().RunText_Unsafe(luaCode);
        }

        /// <summary>
        /// Established whether or not to show a time stamp on all messages.
        /// </summary>
        public static void ShowTimeStamp(bool value)
        {
            if (instance == null)
            {
                return;
            }

            CommandSettings.ShowTimeStamp = value;
            Log("Change successful :D", "green");
        }

        /// <summary>
        /// Sets the console text to the text supplied.
        /// </summary>
        public static void SetText(string text)
        {
            if (instance == null)
            {
                return;
            }

            instance.textArea.text = "\n" + text;
        }

        /// <summary>
        /// Sets the size of the console text.
        /// </summary>
        public static void SetTextSize(int size)
        {
            if (instance == null)
            {
                return;
            }

            instance.textArea.fontSize = size;
        }

        /// <summary>
        /// Passes the Text Object.
        /// </summary>
        public static Text TextObject()
        {
            if (instance == null)
            {
                return null;
            }

            return instance.textArea;
        }

        /// <summary>
        /// Button delegate action to handle command.
        /// </summary>
        public void EnterPressedForInput(Text newValue)
        {
            currentHistoryIndex = -1;

            // Clear input but retrieve
            inputText = newValue.text;
            inputField.text = string.Empty;

            if (inputText != string.Empty)
            {
                // Add Text to log, history then execute
                Log(inputText);
                history.Add(inputText);
                Execute(inputText);
            }
        }

        /// <summary>
        /// Help for all functions.
        /// </summary>
        public void Help()
        {
            string text = string.Empty;

            for (int i = 0; i < consoleCommands.Count; i++)
            {
                text += "\n<color=orange>" + consoleCommands[i].Title + GetParametersWithConsoleMode(consoleCommands[i]) + "</color>" + (consoleCommands[i].DescriptiveText == null ? string.Empty : " //" + consoleCommands[i].DescriptiveText);
            }

            text += "\n<color=orange>Note:</color> If the function has no parameters you <color=red>don't</color> need to use the parameter modifier.";
            text += "\n<color=orange>Note:</color> You <color=red>don't</color> need to use the trailing parameter modifier either";

            Log("-- Help --" + text);
        }

        /// <summary>
        /// Regenerates the autocomplete (button delegate for text field changed).
        /// </summary>
        public void RegenerateAutoComplete(Text newValue)
        {
            inputText = newValue.text;

            if (inputText == string.Empty || consoleCommands == null || consoleCommands.Count == 0)
            {
                return;
            }

            string candidateTester = string.Empty;

            if (possibleCandidates.Count != 0 && selectedCandidate >= 0 && possibleCandidates.Count > selectedCandidate)
            {
                // Set our candidates tester because we had a candidate before and we want it to carry over
                candidateTester = possibleCandidates[selectedCandidate];
            }

            List<string> possible = consoleCommands
                .Where(cB => cB.Title.ToLower().StartsWith(inputText.ToLower()))
                .Select(cB => cB.Title).ToList();

            print(possible);

            if (possible != null)
            {
                possibleCandidates = possible;
            }

            if (candidateTester == string.Empty)
            {
                selectedCandidate = 0;
                return;
            }

            for (int i = 0; i < possibleCandidates.Count; i++)
            {
                if (possibleCandidates[i] == candidateTester)
                {
                    selectedCandidate = i;
                    DirtyAutocomplete();
                    return;
                }
            }

            selectedCandidate = 0;

            DirtyAutocomplete();
        }

        /// <summary>
        /// Initialisation.
        /// </summary>
        private void Awake()
        {
            // Guard
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.ULogWarningChannel("DevConsole", "There can only be one Console per project");
                Destroy(gameObject);
            }

            // Delegation
            SceneManager.activeSceneChanged += SceneChanged;

            KeyboardManager.Instance.RegisterModalInputField(inputField);
            KeyboardManager.Instance.RegisterInputAction("DevConsole", KeyboardMappedInputType.KeyUp, ToggleConsole);
        }

        /// <summary>
        /// Delegation to unload instances.
        /// </summary>
        private void SceneChanged(Scene oldScene, Scene newScene)
        {
            if (instance != null && oldScene != newScene)
            {
                KeyboardManager.Instance.UnRegisterModalInputField(instance.inputField);
                KeyboardManager.Instance.UnRegisterInputAction("DevConsole");

                instance = null;
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Raises the enable event.  Checks if console can open.
        /// </summary>
        private void OnEnable()
        {
            if (CommandSettings.DeveloperConsoleToggle == false || closed)
            {
                Close();
            }
        }

        /// <summary>
        /// Set transform and do guard.
        /// </summary>
        private void Start()
        {
            transform.SetAsLastSibling();

            // Guard
            if (textArea == null || inputField == null || autoComplete == null || scrollRect == null || root == null)
            {
                gameObject.SetActive(false);
                Debug.ULogError("DevConsole", "Missing gameobjects, look at the serializeable fields");
            }

            textArea.fontSize = CommandSettings.FontSize;
            textArea.text = "\n";

            // Load all the commands
            LoadCommands();

            if (root != null)
            {
                Opened = root.activeInHierarchy;
            }
        }

        /// <summary>
        /// Toggles the console on/off.
        /// </summary>
        private void ToggleConsole()
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

        /// <summary>
        /// Late Update, just does some key checking for history / AutoComplete.
        /// </summary>
        private void Update()
        {
            if (!inputField.isFocused)
            {
                currentHistoryIndex = -1;
                selectedCandidate = -1;
                autoComplete.gameObject.SetActive(false);
                return;
            }

            // If input field is focused
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                if (ShowingAutoComplete)
                {
                    // Render autocomplete
                    if (selectedCandidate > 0)
                    {
                        selectedCandidate -= 1;

                        // Re-select
                        DirtyAutocomplete();
                    }
                }
                else if (currentHistoryIndex < (history.Count - 1))
                {
                    // (history.Count -1) is the max index

                    // This way the first index will be the last one so first in last out
                    // Kind of like a stack but you can go up and down it
                    currentHistoryIndex += 1;
                    inputField.text = history[(history.Count - 1) - currentHistoryIndex];
                }

                inputField.MoveTextEnd(false);
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                if (ShowingAutoComplete)
                {
                    // Render autocomplete
                    if (selectedCandidate < (possibleCandidates.Count - 1))
                    {
                        selectedCandidate += 1;

                        // Re-select
                        DirtyAutocomplete();
                    }
                }
                else if (currentHistoryIndex > 0)
                {
                    // This way the first index will be the last one so first in last out
                    // Kind of like a stack but you can go up and down it
                    currentHistoryIndex -= 1;
                    inputField.text = history[history.Count - (currentHistoryIndex + 1)];
                }

                inputField.MoveTextEnd(false);
            }
            else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
            {
                selectedCandidate = -1;
                autoComplete.gameObject.SetActive(false);
            }
            else if (Input.GetKeyUp(KeyCode.Tab))
            {
                if (possibleCandidates.Count == 0)
                {
                    selectedCandidate = -1;
                    autoComplete.gameObject.SetActive(false);
                    inputField.MoveTextEnd(false);
                    return;
                }

                // Handle autocomplete
                if (ShowingAutoComplete)
                {
                    inputField.text = possibleCandidates[selectedCandidate];
                    selectedCandidate = -1;
                    autoComplete.gameObject.SetActive(false);
                    inputField.MoveTextEnd(false);
                }
                else
                {
                    inputField.text = inputField.text.TrimEnd('\t');
                    autoComplete.gameObject.SetActive(true);
                    selectedCandidate = 0;
                    RegenerateAutoComplete(inputField.textComponent);
                }
            }
        }

        /// <summary>
        /// Re-render autocomplete.
        /// </summary>
        private void DirtyAutocomplete()
        {
            print("Dirtied");
            if (selectedCandidate != -1)
            {
                autoComplete.gameObject.SetActive(true);

                // Delete current children
                foreach (Transform child in contentAutoComplete.transform)
                {
                    Destroy(child.gameObject);
                }

                // Recreate from possible candidates
                for (int i = 0; i < possibleCandidates.Count; i++)
                {
                    GameObject go = Instantiate(Resources.Load<GameObject>("UI/Console/DevConsole_AutoCompleteOption"));
                    go.transform.SetParent(contentAutoComplete.transform);

                    // Quick way to add component / text
                    if (i != selectedCandidate)
                    {
                        go.GetComponent<Text>().text = "<color=white>" + possibleCandidates[i] + "</color>";
                    }
                    else
                    {
                        go.GetComponent<Text>().text = "<color=yellow>" + possibleCandidates[i] + "</color>";
                    }
                }
            }
        }

        /// <summary>
        /// Clears commands and re-loads them.
        /// </summary>
        private void LoadCommands()
        {
            consoleCommands.Clear();

            // Load Base Commands
            AddCommands(
                new VectorCommand<Vector3>("ChangeCameraCSharp", ChangeCameraPositionCSharp, "Change Camera Position (Written in CSharp)"),
                new StringCommand("Run_LUA", Run_LUA, "Runs the text as a LUA function"));

            // Load Commands from C#
            // Empty because C# Modding not implemented yet
            // TODO: Once C# Modding Implemented, add the ability to add commands here

            // Load Commands from XML (will be changed to JSON AFTER the current upgrade)
            AddCommands(PrototypeManager.DevConsole.Values.Select(x => x.LUACommand).ToArray());
        }

        private void SetFontSize(int size)
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
    }
}
