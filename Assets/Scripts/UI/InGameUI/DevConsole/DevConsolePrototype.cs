#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using DeveloperConsole.CommandTypes;

namespace DeveloperConsole.Prototypes
{
    [MoonSharpUserData]
    public class LUAPrototype : IPrototypable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LUAPrototype"/> class.
        /// This is required to create a Prototype.
        /// </summary>
        public LUAPrototype()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LUAPrototype"/> class.
        /// This form of the constructor assumes the ScheduledEvent is of the EventType.CSharp type.
        /// </summary>
        /// <param name="title"> The title of the command </param>
        /// <param name="methodFunctionName"> The name of the function to call </param>
        /// <param name="description"> The description of the command </param>
        /// <param name="helpFunctionName"> The name of the function to call to show help </param>
        /// <param name="parameters"> The parameters that this class requires (a string in C# type formats and comma between them) </param>
        public LUAPrototype(string title, string methodFunctionName, string description, string helpFunctionName, string parameters) : this()
        {
            luaCommand = new LUACommand(title, methodFunctionName, description, helpFunctionName, parameters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LUAPrototype"/> class.
        /// This form of the constructor assumes the ScheduledEvent is of the EventType.CSharp type.
        /// </summary>       
        /// <param name="title"> The title of the command </param>
        /// <param name="methodFunctionName"> The name of the function to call </param>
        /// <param name="description"> The description of the command </param>
        public LUAPrototype(string title, string methodFunctionName, string description) : this()
        {
            luaCommand = new LUACommand(title, methodFunctionName, description);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LUAPrototype"/> class.
        /// This form of the constructor assumes the ScheduledEvent is of the EventType.CSharp type.
        /// </summary>   
        /// <param name="title"> The title of the command </param>
        /// <param name="methodFunctionName"> The name of the function to call </param>
        public LUAPrototype(string title, string methodFunctionName) : this()
        {
            luaCommand = new LUACommand(title, methodFunctionName);
        }

        /// <summary>
        /// Copies from other
        /// </summary>
        /// <param name="other"></param>
        public LUAPrototype(LUAPrototype other) : this()
        {
            luaCommand = other.luaCommand;
        }

        /// <summary>
        /// Clones this prototype
        /// </summary>
        /// <returns></returns>
        public LUAPrototype Clone()
        {
            return new LUAPrototype(this);
        }

        /// <summary>
        /// The type of command
        /// </summary>
        public string Type
        {
            get
            {
                return luaCommand.title;
            }
        }

        /// <summary>
        /// Reads from the reader provided
        /// </summary>
        public void ReadXmlPrototype(XmlReader reader)
        {
            string title = reader.GetAttribute("Title");
            string functionName = reader.GetAttribute("FunctionName");
            string description = reader.GetAttribute("Description");
            string helpFunctionName = reader.GetAttribute("HelpFunctionName");
            string parameters = reader.GetAttribute("Parameters");

            luaCommand = new LUACommand(title, functionName, description, helpFunctionName, parameters);
        }

        /// <summary>
        /// Writes to the writer provided
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Title", luaCommand.title);
            writer.WriteAttributeString("FunctionName", luaCommand.functionName);
            writer.WriteAttributeString("Description", luaCommand.descriptiveText);
            writer.WriteAttributeString("HelpFunctionName", luaCommand.helpFunctionName);
            writer.WriteAttributeString("Parameters", luaCommand.parameters);
        }

        /// <summary>
        /// The main data storage of luacommand
        /// </summary>
        public LUACommand luaCommand { get; protected set; }
    }
}
