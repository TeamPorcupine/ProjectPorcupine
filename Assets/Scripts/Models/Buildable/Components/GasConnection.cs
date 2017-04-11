#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProjectPorcupine.Rooms;

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    [XmlRoot("Component")]
    [BuildableComponentName("GasConnection")]
    public class GasConnection : BuildableComponent
    {
        public GasConnection()
        {
        }

        private GasConnection(GasConnection other) : base(other)
        {
            Provides = other.Provides;
            Requires = other.Requires;
        }

        [XmlElement("Provides")]
        [JsonProperty("Provides")]
        public List<GasInfo> Provides { get; set; }

        [XmlElement("Requires")]
        [JsonProperty("Requires")]
        public List<GasInfo> Requires { get; set; }

        [XmlElement("Efficiency")]
        [JsonProperty("Efficiency")]
        public SourceDataInfo Efficiency { get; set; }

        public override bool RequiresSlowUpdate
        {
            get
            {
                return true;
            }
        }

        public override BuildableComponent Clone()
        {
            return new GasConnection(this);
        }

        public override bool CanFunction()
        {
            bool canFunction = true;

            // check if all requirements are fullfilled
            if (Requires != null && Requires.Count > 0)
            {
                Room room = ParentFurniture.Tile.Room;
                foreach (GasInfo reqGas in Requires)
                {
                    // get actual gas pressure so it matches what gas really is, not the prettified version for display
                    float curGasPressure = room.GetGasPressure(reqGas.Gas);
                    if (curGasPressure < reqGas.MinLimit || curGasPressure > reqGas.MaxLimit)
                    {
                        canFunction = false;
                    }
                }
            }

            return canFunction;
        }

        public override void FixedFrequencyUpdate(float deltaTime)
        {
            float efficiency = 1f;
            if (Efficiency != null)
            {
                efficiency = RetrieveFloatFor(Efficiency, ParentFurniture);
            }

            if (Provides != null && Provides.Count > 0)
            {
                Room room = ParentFurniture.Tile.Room;
                foreach (GasInfo provGas in Provides)
                {
                    // get actual gas pressure so it matches what gas really is, not the prettified version for display
                    float curGasPressure = room.GetGasPressure(provGas.Gas);
                    if ((provGas.Rate > 0 && curGasPressure < provGas.MaxLimit) ||
                        (provGas.Rate < 0 && curGasPressure > provGas.MinLimit))
                    {
                        // TODO Impose a max limit to avoid rounding errors
                        // TODO Implement gas networks as their own AtmosphereComponent so temperature is consistent
                        room.Atmosphere.CreateGas(provGas.Gas, provGas.Rate * deltaTime, 0.0f);
                    }
                }
            }
        }

        protected override void Initialize()
        {
            componentRequirements = Requirements.Gas;
        }

        [Serializable]
        [JsonObject(MemberSerialization.OptOut)]
        public class GasInfo
        {
            public GasInfo()
            {
                // make sure max. limit is bigger than min. limit
                MaxLimit = 1f;
            }

            [XmlAttribute("gas")]
            public string Gas { get; set; }

            [XmlAttribute("rate")]
            public float Rate { get; set; }

            [XmlAttribute("minLimit")]
            public float MinLimit { get; set; }

            [XmlAttribute("maxLimit")]
            public float MaxLimit { get; set; }

            public override string ToString()
            {
                return string.Format("gas:{0}, rate:{1}, min:{2}, max:{3}", Gas, Rate, MinLimit, MaxLimit);
            }
        }
    }
}
