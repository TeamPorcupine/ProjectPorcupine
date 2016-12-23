#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Xml;
using DeveloperConsole.CommandTypes;
using MoonSharp.Interpreter;

namespace DeveloperConsole
{
    /// <summary>
    /// A prototype for console commands.
    /// </summary>
    [MoonSharpUserData]
    public class CommandPrototype : IPrototypable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPrototype"/> class.
        /// This is required to create a Prototype.
        /// </summary>
        public CommandPrototype()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPrototype"/> class.
        /// This form of the constructor assumes the ScheduledEvent is of the EventType.CSharp type.
        /// </summary>
        /// <param name="title"> The title of the command.</param>
        /// <param name="methodFunctionName"> The name of the function to call.</param>
        /// <param name="description"> The description of the command.</param>
        /// <param name="helpFunctionName"> The name of the function to call to show help.</param>
        /// <param name="parameters"> The parameters that this class requires (a string in C# type formats and comma between them).</param>
        public CommandPrototype(string title, string methodFunctionName, string description, string helpFunctionName, string parameters, string tags, string defaultValue) : this()
        {
            ConsoleCommand = new InvokeCommand(title, methodFunctionName, description, helpFunctionName, parameters, tags.Split(','), defaultValue);
        }

        /// <summary>
        /// Copies from other.
        /// </summary>
        public CommandPrototype(CommandPrototype other) : this()
        {
            ConsoleCommand = other.ConsoleCommand;
        }

        /// <summary>
        /// The type of command.
        /// </summary>
        public string Type
        {
            get
            {
                return ConsoleCommand.Title;
            }
        }

        /// <summary>
        /// The main data storage of luacommand.
        /// </summary>
        public InvokeCommand ConsoleCommand { get; protected set; }

        /// <summary>
        /// Clones this prototype.
        /// </summary>
        public CommandPrototype Clone()
        {
            return new CommandPrototype(this);
        }

        /// <summary>
        /// Reads from the reader provided.
        /// </summary>
        public void ReadXmlPrototype(XmlReader reader)
        {
            string title = reader.GetAttribute("Title");
            string functionName = reader.GetAttribute("FunctionName");
            string description = reader.GetAttribute("Description");
            string helpFunctionName = reader.GetAttribute("HelpFunctionName");
            string parameters = reader.GetAttribute("Parameters");
            string tags = reader.GetAttribute("Tags");

            string defaultValue = reader.GetAttribute("DefaultValue");

            // This is an optional checker basically
            if (tags == null)
            {
                tags = string.Empty;
            }

            if (defaultValue == null)
            {
                defaultValue = string.Empty;
            }

            ConsoleCommand = new InvokeCommand(title, functionName, description, helpFunctionName, parameters, tags.Split(','), defaultValue);
        }
    }
}
