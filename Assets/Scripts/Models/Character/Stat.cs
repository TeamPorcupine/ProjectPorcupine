#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Xml;

public class Stat : IPrototypable
{
    public Stat()
    {
    }

    private Stat(Stat other)
    {
        Type = other.Type;
        Name = other.Name;
    }

    public string Type { get; set; }

    public string Name { get; set; }

    public int Value { get; set; }

    public void ReadXmlPrototype(XmlReader parentReader)
    {
        Type = parentReader.GetAttribute("type");
        Name = parentReader.GetAttribute("name");
    }

    public Stat Clone()
    {
        return new Stat(this);
    }

    public override string ToString()
    {
        return string.Format("{0}: {1}", Type, Value);
    }
}
