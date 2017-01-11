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
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using ProjectPorcupine.Localization;
using ProjectPorcupine.Rooms;

namespace ProjectPorcupine.Buildable.Components
{
    /// <summary>
    /// Outputs heat, but can also be set to require a certain heat to function.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    [XmlRoot("Component")]
    [BuildableComponentName("TemperatureConnection")]
    [MoonSharpUserData]
    public class TemperatureComponent : BuildableComponent
    {
        public TemperatureComponent()
        {
        }

        private TemperatureComponent(TemperatureComponent other) : base(other)
        {
            Requires = other.Requires;
            Outputs = other.Outputs;
            ParamsDefinitions = other.ParamsDefinitions;
        }

        [XmlElement("Requires")]
        [JsonProperty("Requires")]
        public InputInfo Requires { get; set; }

        [XmlElement("Outputs")]
        [JsonProperty("Outputs")]
        public OutputInfo Outputs { get; set; }

        [XmlElement("ParameterDefinitions")]
        [JsonProperty("ParameterDefinitions")]
        public TemperatureConnectionParameterDefinitions ParamsDefinitions { get; set; }

        public override bool RequiresSlowUpdate
        {
            get
            {
                return true;
            }
        }

        public float OutputRate
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.OutputRate.ParameterName].ToFloat();
            }

            set
            {
                FurnitureParams[ParamsDefinitions.OutputRate.ParameterName].SetValue(value);
            }
        }

        public override BuildableComponent Clone()
        {
            return new TemperatureComponent(this);
        }

        public override bool CanFunction()
        {
            bool canFunction = true;

            // check if all requirements are fullfilled
            if (Requires != null)
            {
                if (ParentFurniture.Tile.TemperatureUnit.TemperatureInKelvin < Requires.MinLimit || ParentFurniture.Tile.TemperatureUnit.TemperatureInKelvin > Requires.MaxLimit)
                {
                    canFunction = false;
                }
            }

            if (Outputs != null)
            {

            }

            return canFunction;
        }

        public override void FixedFrequencyUpdate(float deltaTime)
        {
            if (Outputs != null && OutputRate != 0)
            {
                if (Outputs.MaxLimit != -1)
                {
                    if (ParentFurniture.Tile.Room != null && ParentFurniture.Tile.TemperatureUnit.TemperatureInKelvin > Outputs.MaxLimit)
                    {
                        return;
                    }
                }

                if (Outputs.MinLimit != -1)
                {
                    if (ParentFurniture.Tile.Room != null && ParentFurniture.Tile.TemperatureUnit.TemperatureInKelvin < Outputs.MinLimit)
                    {
                        return;
                    }
                }

                // Else we are fine :D
                World.Current.temperature.ProduceTemperatureAtFurniture(ParentFurniture, OutputRate, deltaTime);
            }
        }

        protected override void Initialize()
        {
            componentRequirements = Requirements.Temperature;

            if (ParamsDefinitions == null)
            {
                // don't need definition for all furniture, just use defaults
                ParamsDefinitions = new TemperatureConnectionParameterDefinitions();
            }

            if (Outputs != null)
            {
                OutputRate = Outputs.InitialOutput;
            }
        }

        [Serializable]
        [JsonObject(MemberSerialization.OptOut)]
        public class InputInfo
        {
            public InputInfo()
            {
                MinLimit = -1;
                MaxLimit = -1;
            }

            [XmlAttribute("minLimit")]
            public float MinLimit { get; set; }

            [XmlAttribute("maxLimit")]
            public float MaxLimit { get; set; }

            public override string ToString()
            {
                return string.Format("Temperature min:{0}, max:{1}", MinLimit, MaxLimit);
            }
        }

        [Serializable]
        [JsonObject(MemberSerialization.OptOut)]
        public class TemperatureConnectionParameterDefinitions
        {
            // constants for parameters
            public const string OutputRateParamName = "temperature_outputRate";

            public TemperatureConnectionParameterDefinitions()
            {
                // defaults
                OutputRate = new ParameterDefinition(OutputRateParamName);
            }

            public ParameterDefinition OutputRate { get; set; }
        }

        [Serializable]
        [JsonObject(MemberSerialization.OptOut)]
        public class OutputInfo : InputInfo
        {
            public OutputInfo()
            {
                MinLimit = -1;
                MaxLimit = -1;
                InitialOutput = 0;
            }

            [XmlAttribute("initialOutput")]
            public float InitialOutput { get; set; }
        }
    }
}
