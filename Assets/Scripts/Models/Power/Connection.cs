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
using MoonSharp.Interpreter;

namespace Power
{
    /// <summary>
    /// Represents connection to electric grid if furniture has connection specified it uses of produce power.
    /// </summary>
    [MoonSharpUserData]
    public class Connection : IXmlSerializable
    {
        private readonly string inputRateAttributeName = "inputRate";
        private readonly string outputRateAttributeName = "outputRate";
        private readonly string capacityAttributeName = "capacity";
        private readonly string accumulatedPowerAttributeName = "accumulatedPower";

        /// <summary>
        /// Amount of power consumed by this connection per Tick of system
        /// Accumulator: rate of charge.
        /// </summary>
        public float InputRate { get; set; }

        /// <summary>
        /// Amount of power produced by this connection per Tick of system
        /// Accumulator: rate of discharge.
        /// </summary>
        public float OutputRate { get; set; }

        /// <summary>
        /// Accumulator only: amount of power that could be stored.
        /// </summary>
        public float Capacity { get; set; }

        /// <summary>
        /// Accumulator only: amount of power that is stored.
        /// </summary>
        public float AccumulatedPower { get; set; }

        public bool IsEmpty
        {
            get { return AccumulatedPower.IsZero(); }
        }

        public bool IsFull
        {
            get { return AccumulatedPower.AreEqual(Capacity); }
        }

        public bool IsPowerProducer
        {
            get { return InputRate.IsZero() && OutputRate > 0.0f; }
        }

        public bool IsPowerConsumer
        {
            get { return OutputRate.IsZero() && InputRate > 0.0f; }
        }

        public bool IsPowerAccumulator
        {
            get { return Capacity > 0.0f; }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            InputRate = RaedFloatNullAsZero(reader.GetAttribute(inputRateAttributeName));
            OutputRate = RaedFloatNullAsZero(reader.GetAttribute(outputRateAttributeName));
            Capacity = RaedFloatNullAsZero(reader.GetAttribute(capacityAttributeName));
            AccumulatedPower = RaedFloatNullAsZero(reader.GetAttribute(accumulatedPowerAttributeName));
        }

        public void ReadPrototype(XmlReader reader)
        {
            InputRate = RaedFloatNullAsZero(reader.GetAttribute(inputRateAttributeName));
            OutputRate = RaedFloatNullAsZero(reader.GetAttribute(outputRateAttributeName));
            Capacity = RaedFloatNullAsZero(reader.GetAttribute(capacityAttributeName));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(inputRateAttributeName, InputRate.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(outputRateAttributeName, OutputRate.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(capacityAttributeName, Capacity.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(accumulatedPowerAttributeName, AccumulatedPower.ToString(CultureInfo.InvariantCulture));
        }

        private static float RaedFloatNullAsZero(string value)
        {
            float result;
            if (string.IsNullOrEmpty(value))
            {
                return 0.0f;
            }

            return float.TryParse(value, out result) ? result : 0.0f;
        }
    }
}
