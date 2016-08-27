#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Xml;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class QuestReward
{
    public string Description;
    public string OnRewardLuaFunction;
    public Parameter Parameters;
    public bool IsCollected;

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        Description = reader_parent.GetAttribute("Description");
        OnRewardLuaFunction = reader_parent.GetAttribute("OnRewardLuaFunction");

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