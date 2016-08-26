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