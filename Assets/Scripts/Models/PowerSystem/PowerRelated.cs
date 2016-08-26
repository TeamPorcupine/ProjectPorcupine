#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class PowerRelated : IXmlSerializable
{
    private readonly string inputRateAttributeName = "inputRate";
    private readonly string outputRateAttributeName = "outputRate";
    private readonly string capacityAttributeName = "capacity";
    private readonly string accumulatedPowerAttributeName = "accumulatedPower";

    public float InputRate { get; set; }

    public float OutputRate { get; set; }

    public float Capacity { get; set; }

    public float AccumulatedPower { get; set; }

    public bool IsEmpty
    {
        get
        {
            return AccumulatedPower.IsZero();
        }
    }

    public bool IsFull
    {
        get
        {
            return AccumulatedPower.AreEqual(Capacity);
        }
    }

    public bool IsPowerProducer
    {
        get
        {
            return InputRate.IsZero() && OutputRate > 0.0f;
        }
    }

    public bool IsPowerConsumer
    {
        get
        {
            return OutputRate.IsZero() && InputRate > 0.0f;
        }
    }

    public bool IsPowerAccumulator
    {
        get
        {
            return Capacity > 0.0f;
        }
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        InputRate = float.Parse(reader.GetAttribute(inputRateAttributeName));
        OutputRate = float.Parse(reader.GetAttribute(outputRateAttributeName));
        Capacity = float.Parse(reader.GetAttribute(capacityAttributeName));
        AccumulatedPower = float.Parse(reader.GetAttribute(accumulatedPowerAttributeName));
    }

    public void ReadPrototype(XmlReader reader)
    {
        InputRate = float.Parse(reader.GetAttribute(inputRateAttributeName));
        OutputRate = float.Parse(reader.GetAttribute(outputRateAttributeName));
        Capacity = float.Parse(reader.GetAttribute(capacityAttributeName));
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString(inputRateAttributeName, InputRate.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString(outputRateAttributeName, OutputRate.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString(capacityAttributeName, Capacity.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString(accumulatedPowerAttributeName, AccumulatedPower.ToString(CultureInfo.InvariantCulture));
    }
}
