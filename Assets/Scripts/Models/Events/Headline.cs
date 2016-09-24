#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Xml;

/// <summary>
/// A class that holds each Headline.
/// </summary>
public class Headline : IPrototypable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Headline"/> class.
    /// </summary>
    public Headline()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Headline"/> class.
    /// </summary>
    /// <param name="text">The headline text.</param>
    public Headline(string text)
    {
        Text = text;
    }

    /// <summary>
    /// A key that is used in the Protptype map. For now is just the text.
    /// </summary>
    /// <value>The headline type.</value>
    public string Type
    {
        get { return Text; }
    }

    /// <summary>
    /// Gets the headline text.
    /// </summary>
    /// <value>The headline text.</value>
    public string Text { get; private set; }

    /// <summary>
    /// Reads the prototype from the specified XML reader.
    /// </summary>
    /// <param name="Reader">The XML reader to read from.</param>
    public void ReadXmlPrototype(XmlReader reader)
    {
        reader.Read();
        Text = reader.ReadContentAsString();
    }
}
