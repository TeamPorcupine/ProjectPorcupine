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
using UnityEngine.UI;

namespace DeveloperConsole
{
    public delegate void HelpMethod();

    [MoonSharpUserData]
    public class DevConsole : MonoBehaviour
    {
        /// <summary>
        /// Max lines before cleaning.
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
            instance.inputField.ActivateInputField();
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

            BasePrint("<color=" + (color.ToLower() != "green" ? color : "#7CFC00") + ">" + obj.ToString() + "</color>");
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
            if (instance.textArea.cachedTextGenerator.characterCount >= AutoclearThreshold || instance.textArea.cachedTextGenerator.vertexCount > 55000)
            {
                instance.textArea.text = "\nAUTO-CLEAR\n";
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

            // We want to ONLY split once!
            string[] methodNameAndArgs = command.Trim().TrimEnd(')').Split(new char[] { '(' }, 2);
            string method = command;
            string args = string.Empty;

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
                    ShowHelpMethod(commandToCall);
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
            }
            else
            {
                Log("<color=yellow>Command Info:</color> " + ((help.DescriptiveText == string.Empty) ? " < color=red>There's no help for this command</color>" : help.DescriptiveText));
            }

            Log("<color=yellow>Call it like </color><color=orange> " + help.Title + GetParameters(help) + "</color>");
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
        /// Returns a parameter list using the correct command mode.
        /// </summary>
        /// <param name="description"> The description of the command.</param>
        public static string GetParameters(CommandBase description)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            return " (" + description.Parameters + ")";
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
        /// Refreshes variables based on the settings.
        /// </summary>
        public static void DirtySettings()
        {
            if (instance == null)
            {
                return;
            }

            instance.textArea.fontSize = CommandSettings.FontSize;
            instance.scrollRect.scrollSensitivity = CommandSettings.ScrollingSensitivity;
        }

        /// <summary>
        /// Button delegate action to handle command.
        /// </summary>
        public void EnterPressedForInput(Text newValue)
        {
            inputField.ActivateInputField();

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

            if (possibleCandidates.Count == 0)
            {
                selectedCandidate = -1;
                autoComplete.gameObject.SetActive(false);
            }
            else
            {
                selectedCandidate = 0;
            }

            DirtyAutocomplete();
        }

        /// <summary>
        /// Raises the enable event.  It just does some instance checking.
        /// </summary>
        private void OnEnable()
        {
            // Guard
            if (instance != this && instance != null)
            {
                // Destroy instance.
                UnityDebugger.Debugger.LogError("DevConsole", "There can only be one Console per project.  Deleting instance with name: " + instance.gameObject.name);
                Destroy(instance.gameObject);
            }

            // Saves a small amount of extra calls and what not
            if (instance != this)
            {
                instance = this;

                KeyboardManager.Instance.RegisterModalInputField(inputField);
                KeyboardManager.Instance.RegisterInputAction("DevConsole", KeyboardMappedInputType.KeyUp, ToggleConsole);
            }

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
                UnityDebugger.Debugger.LogError("DevConsole", "Missing gameobjects, look at the serializeable fields");
            }

            textArea.fontSize = CommandSettings.FontSize;
            textArea.text = "\n";

            // Load all the commands
            LoadCommands();

            if (root != null)
            {
                Opened = root.activeInHierarchy;

                if (Opened)
                {
                    inputField.ActivateInputField();
                }
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

            KeyboardManager.Instance.TriggerActionIfValid("DevConsole");

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
            else
            {
                autoComplete.gameObject.SetActive(false);
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
                new Command("Help", CoreCommands.Help, "Returns information on all commands"),
                new Command<Vector3>("ChangeCameraPosition", CoreCommands.ChangeCameraPosition, "Change Camera Position (Written in CSharp)"),
                new Command<string>("Run_LUA", CoreCommands.Run_LUA, "Runs the text as a LUA function"),
                new Command<string>("SetText", CoreCommands.SetText, "Sets the devConsole text"),
                new Command("Clear", CoreCommands.Clear, "Clears the developer console"),
                new Command<string, float>("SetCharacterHealth", CoreCommands.SetCharacterHealth, "Sets the character's health"),
                new Command<string, float>("DamageCharacter", CoreCommands.DamageCharacter, "Damages said character"),
                new Command<string>("ClearCharStateQueue", CoreCommands.CharacterClearStateQueue, "Clears the said character's state queue"),
                new Command("ClearAllCharStateQueue", CoreCommands.AllCharactersClearStateQueue, "Clears all chaaracter's state queue"),
                new Command<string, float>("AddCurrency", CoreCommands.AddCurrency, "Adds x amount of currency to wallet"),
                new Command<Vector3, int>("ConsumeInventory", CoreCommands.ConsumeInventory, "Consumes x amount of inventory at position provided"),
                new Command<Vector3, string, Vector2>("PlaceInventoryPos", CoreCommands.PlaceInventory, "Places inventory at position provided"),
                new Command<string, int, string, Vector2>("PlaceInventoryChar", CoreCommands.PlaceInventory, "Places inventory at character provided"),
                new Command<string, int, bool>("RemoveInventoryOfType", CoreCommands.RemoveInventoryOfType, "Remove inventory of type x"),
                new Command<string, Vector3, float>("PlaceFurniture", CoreCommands.PlaceFurniture, "Places furniture"),
                new Command<string, Vector3>("IsWorkSpotClear", CoreCommands.IsWorkSpotClear, "Is the work spot at the position provided clear"),
                new Command<string, Vector3, float>("IsPlacementValid", CoreCommands.IsPlacementValid, "Is the spot provided available for furniture"),
                new Command<Vector3>("GetTemperature", CoreCommands.GetTemperature, "Logs the current temperature at the square provided"),
                new Command<Vector3>("GetThermalDiffusivity", CoreCommands.GetThermallDiffusivity, "Logs the current Thermal Diffusivity at the square provided"),
                new Command<Vector3>("FloodFillRoom", CoreCommands.FloodFillRoomAt, "Flood fills the room at the position provided"),
                new Command("GetRoomIDs", CoreCommands.GetAllRoomIDs, "Logs all the room IDs"),
                new Command<int, string, Vector3>("DoBuild", CoreCommands.DoBuild, CoreCommands.DoBuildHelp),
                new Command("DirtyTileGraph", CoreCommands.InvalidateTileGraph, "Invalidates the tile graph"),
                new Command("GetCharNames", CoreCommands.GetCharacterNames, "Logs all the character names"),
                new Command<int, string, float>("SetRoomGas", CoreCommands.SetRoomGas, "Sets the gas in the room"),
                new Command<string, float>("SetAllRoomsGas", CoreCommands.SetAllRoomsGas, "Sets the gas in all rooms"),
                new Command<int>("FillRoomWithAir", CoreCommands.FillRoomWithAir, "Set's the room's gasses to standard atmosphere (20/80 O2/N2 mix)"),
                new Command("FillAllRoomsWithAir", CoreCommands.FillAllRoomsWithAir, "Set's all rooms' gasses to standard atmosphere (20/80 O2/N2 mix)"),
                new Command<int>("EmptyRoom", CoreCommands.EmptyRoom, "Empties the room's atmosphere"),
                new Command("EmptyAllRooms", CoreCommands.EmptyAllRooms, "Empties all rooms' atmosphere"));

            // Load Commands from XML (will be changed to JSON AFTER the current upgrade)
            // Covers both CSharp and LUA
            AddCommands(PrototypeManager.DevConsole.Values.Select(x => x.ConsoleCommand).ToArray());
        }
    }
}
