#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Diagnostics;
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
    [DebuggerDisplay("ConnectionKey = {ConnectionKey}")]
    public class Connection : IXmlSerializable, IEquatable<Connection>
    {
        private readonly string inputRateAttributeName = "inputRate";
        private readonly string outputRateAttributeName = "outputRate";
        private readonly string capacityAttributeName = "capacity";
        private readonly string accumulatedPowerAttributeName = "accumulatedPower";
        private readonly string powerGeneratorKey = "PowerGenerator";
        private readonly string powerConsumerKey = "PowerConsumer";
        private readonly string accumulatorKey = "Accumulator";
        private readonly string uniqueConnectionKey;
        private readonly string baseConnectionKey;
        private string connectionKey;

        public Connection()
        {
            uniqueConnectionKey = string.Format("{0:yyyyMMddHHmmssfffff}", DateTime.Now);
        }

        public Connection(string baseConnectionKey)
        {
            this.baseConnectionKey = baseConnectionKey;
            uniqueConnectionKey = null;
        }

        public Connection(Connection connection) : this()
        {
            InputRate = connection.InputRate;
            OutputRate = connection.OutputRate;
            Capacity = connection.Capacity;
            baseConnectionKey = GetBaseConnectionKey();
        }

        private string ConnectionKey
        {
            get
            {
                if (string.IsNullOrEmpty(connectionKey))
                {
                    connectionKey = string.IsNullOrEmpty(uniqueConnectionKey) ? baseConnectionKey :
                        string.Format("{0}_{1}", baseConnectionKey, uniqueConnectionKey);
                }

                return connectionKey;
            }
        }

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

        public bool Equals(Connection other)
        {
            if (other == null)
            {
                return false;
            }

            return ConnectionKey != null && ConnectionKey.Equals(other.connectionKey) &&
                   InputRate.AreEqual(other.InputRate) &&
                   OutputRate.AreEqual(other.OutputRate) &&
                   Capacity.AreEqual(other.Capacity);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Connection);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ConnectionKey != null ? ConnectionKey.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ InputRate.GetHashCode();
                hashCode = (hashCode * 397) ^ OutputRate.GetHashCode();
                hashCode = (hashCode * 397) ^ Capacity.GetHashCode();
                return hashCode;
            }
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

        private string GetBaseConnectionKey()
        {
            if (IsPowerAccumulator)
            {
                return accumulatorKey;
            }

            return IsPowerProducer ? powerGeneratorKey : powerConsumerKey;
        }
    }
}
