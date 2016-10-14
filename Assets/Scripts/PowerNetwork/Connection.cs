#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;

namespace ProjectPorcupine.PowerNetwork
{
    /// <summary>
    /// Represents connection to electric grid if furniture has connection specified it uses of produce power.
    /// </summary>
    [MoonSharpUserData]
    public class Connection : IXmlSerializable
    {
        private static readonly string InputRateAttributeName = "inputRate";
        private static readonly string OutputRateAttributeName = "outputRate";
        private static readonly string CapacityAttributeName = "capacity";
        private static readonly string AccumulatedPowerAttributeName = "accumulatedPower";

        private readonly double[] capacityThresholds = new[] { 0.0, 0.25, 0.5, 0.75, 1.0 };

        private int currentThresholdIndex = 0;
        private float accumulatedPower;

        public event Action<Connection> NewThresholdReached;

        public event Action Reconnecting;

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
        public float AccumulatedPower
        {
            get
            {
                return accumulatedPower;
            }

            set
            {
                if (accumulatedPower.AreEqual(value))
                {
                    return;
                }

                float oldAccumulatedPower = accumulatedPower;
                accumulatedPower = value;
                if (IsNewThresholdReached(oldAccumulatedPower))
                {
                    OnNewThresholdReached(this);
                }
            }
        }

        /// <summary>
        /// Currently reached capacity threshold in %.
        /// </summary>
        public int CurrentThreshold { get; set; }

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

        public void Reconnect()
        {
            if (Reconnecting != null)
            {
                Reconnecting();
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            InputRate = ReadFloatNullAsZero(reader.GetAttribute(InputRateAttributeName));
            OutputRate = ReadFloatNullAsZero(reader.GetAttribute(OutputRateAttributeName));
            Capacity = ReadFloatNullAsZero(reader.GetAttribute(CapacityAttributeName));
            AccumulatedPower = ReadFloatNullAsZero(reader.GetAttribute(AccumulatedPowerAttributeName));
        }

        public void ReadPrototype(XmlReader reader)
        {
            InputRate = ReadFloatNullAsZero(reader.GetAttribute(InputRateAttributeName));
            OutputRate = ReadFloatNullAsZero(reader.GetAttribute(OutputRateAttributeName));
            Capacity = ReadFloatNullAsZero(reader.GetAttribute(CapacityAttributeName));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(InputRateAttributeName, InputRate.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(OutputRateAttributeName, OutputRate.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(CapacityAttributeName, Capacity.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(AccumulatedPowerAttributeName, AccumulatedPower.ToString(CultureInfo.InvariantCulture));
        }

        public Connection Clone()
        {
            return new Connection
            {
                InputRate = InputRate,
                OutputRate = OutputRate,
                Capacity = Capacity,
                AccumulatedPower = AccumulatedPower
            };
        }

        private static float ReadFloatNullAsZero(string value)
        {
            float result;
            if (string.IsNullOrEmpty(value))
            {
                return 0.0f;
            }

            return float.TryParse(value, out result) ? result : 0.0f;
        }

        private bool IsNewThresholdReached(float oldAccumulatedPower)
        {
            int thresholdMovingDirection = oldAccumulatedPower < accumulatedPower ? 1 : -1;
            int nextThesholdIndex = (currentThresholdIndex + thresholdMovingDirection).Clamp(0, capacityThresholds.Length - 1);
            double nextThreshold = capacityThresholds[nextThesholdIndex];

            if ((thresholdMovingDirection > 0 && accumulatedPower >= nextThreshold * Capacity) ||
                (thresholdMovingDirection < 0 && accumulatedPower <= nextThreshold * Capacity))
            {
                currentThresholdIndex = nextThesholdIndex;
                CurrentThreshold = (int)(nextThreshold * 100);
                return true;
            }

            return false;
        }

        private void OnNewThresholdReached(Connection connection)
        {
            Action<Connection> handler = NewThresholdReached;
            if (handler != null)
            {
                handler(connection);
            }
        }
    }
}
