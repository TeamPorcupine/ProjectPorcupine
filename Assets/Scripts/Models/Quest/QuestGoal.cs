#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Security.Cryptography;
using System.Xml;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class QuestGoal
{
    public string Description;
    public string IsCompletedLuaFunction;
    public Parameter Parameters;
    public bool IsCompleted;

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        Description = reader_parent.GetAttribute("Description");
        IsCompletedLuaFunction = reader_parent.GetAttribute("IsCompletedLuaFunction");

        XmlReader reader = reader_parent.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Params":
                    Parameters = Parameter.ReadXml(reader);
                    break;
            }
        }
    }
}