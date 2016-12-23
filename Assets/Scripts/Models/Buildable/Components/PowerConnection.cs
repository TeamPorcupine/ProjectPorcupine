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
using ProjectPorcupine.PowerNetwork;

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    [XmlRoot("Component")]
    [BuildableComponentName("PowerConnection")]
    [MoonSharpUserData]
    public class PowerConnection : BuildableComponent, IPluggable
    {       
        public PowerConnection()
        {
        }

        private PowerConnection(PowerConnection other) : base(other)
        {
            ParamsDefinitions = other.ParamsDefinitions;
            Provides = other.Provides;
            Requires = other.Requires;

            Reconnecting += OnReconnecting;
        }

        public event Action Reconnecting;

        [XmlElement("ParameterDefinitions")]
        [JsonProperty("ParameterDefinitions")]
        public PowerConnectionParameterDefinitions ParamsDefinitions { get; set; }

        public Parameter CurrentAccumulatorCharge
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.CurrentAcumulatorCharge.ParameterName];
            }
        }

        public Parameter CurrentAccumulatorChargeIndex
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.CurrentAcumulatorChargeIndex.ParameterName];
            }
        }

        [XmlIgnore]
        public bool IsRunning
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.IsRunning.ParameterName].ToBool();
            }

            set
            {
                FurnitureParams[ParamsDefinitions.IsRunning.ParameterName].SetValue(value);
            }
        }

        [XmlElement("Provides")]
        [JsonProperty("Provides")]
        public Info Provides { get; set; }

        [XmlElement("Requires")]
        [JsonProperty("Requires")]
        public Info Requires { get; set; }
        
        [XmlIgnore]
        public float StoredAmount
        {
            get
            {
                return CurrentAccumulatorCharge.ToFloat();
            }

            set
            {
                float oldAccumulatorCharge = CurrentAccumulatorCharge.ToFloat();
                if (oldAccumulatorCharge != value)
                {
                    CurrentAccumulatorCharge.SetValue(value);

                    int curIndex = (int)((Provides.CapacityThresholds - 1) * (value / Provides.Capacity));
                    CurrentAccumulatorChargeIndex.SetValue(curIndex);
                }
            }
        }

        public float InputRate
        {
            get { return Requires.Rate; }
        }

        public float OutputRate
        {
            get { return Provides.Rate; }
        }

        public bool IsEmpty
        {
            get { return IsStorage && StoredAmount.IsZero(); }
        }

        public string UtilityType 
        { 
            get 
            { 
                return "Power";
            }
        }

        public string SubType
        {
            get
            {
                return string.Empty;
            }

            set
            {
            }
        }
        
        public bool IsFull
        {
            get { return IsStorage && StoredAmount.AreEqual(Provides.Capacity); }
        }

        public bool IsProducer
        {
            get { return Provides != null && IsRunning && Provides.Rate > 0.0f && !IsStorage; }
        }

        public bool IsConsumer
        {
            get { return Requires != null && Requires.Rate > 0.0f && !IsStorage; }
        }

        public bool IsStorage
        {
            get { return Provides != null && Provides.Capacity > 0.0f; }
        }

        public float StorageCapacity
        {
            get { return IsStorage ? Provides.Capacity : 0f; }
        }

        public override BuildableComponent Clone()
        {
            return new PowerConnection(this);
        }

        public override bool CanFunction()
        {
            bool hasPower = true;
            if (IsConsumer)
            {
                hasPower = World.Current.PowerNetwork.HasPower(this);
            }

            return hasPower;
        }

        public override void FixedFrequencyUpdate(float deltaTime)
        {
            bool areAllParamReqsFulfilled = true;
            if (Requires != null)
            {
                areAllParamReqsFulfilled = AreParameterConditionsFulfilled(Requires.ParamConditions);
            }

            if (IsStorage)
            {
                areAllParamReqsFulfilled &= StoredAmount > 0;
            }

            IsRunning = areAllParamReqsFulfilled;
        }
        
        public override IEnumerable<string> GetDescription()
        {           
            string powerColor = IsRunning ? "lime" : "red";
            string status = IsRunning ? "online" : "offline";
            yield return LocalizationTable.GetLocalization("power_grid_status_" + status, powerColor);

            if (IsConsumer && Requires != null)
            {
                yield return LocalizationTable.GetLocalization("power_input_status", powerColor, Requires.Rate);
            }

            if (IsProducer && Requires != null)
            {
                yield return LocalizationTable.GetLocalization("power_output_status", powerColor, Requires.Rate);
            }

            if (IsStorage && Provides != null)
            {
                yield return LocalizationTable.GetLocalization("power_accumulated_fraction", StoredAmount, Provides.Capacity);
            }
        }

        public void Reconnect()
        {
            if (Reconnecting != null)
            {
                Reconnecting();
            }
        }

        protected override void Initialize()
        {
            componentRequirements = Requirements.Power;

            if (ParamsDefinitions == null)
            {
                // don't need definition for all furniture, just use defaults
                ParamsDefinitions = new PowerConnectionParameterDefinitions();
            }

            // need to keep accumulator current charge in parameters

            // TODO: need to prevent defining same type more than once?
            if (Provides != null)
            {
                if (Provides.Capacity > 0)
                {
                    CurrentAccumulatorCharge.SetValue(0f);
                    CurrentAccumulatorChargeIndex.SetValue(0);
                }
            }

            IsRunning = false;

            OnReconnecting();

            ParentFurniture.Removed += PowerConnectionRemoved;           
        }
        
        private void PowerConnectionRemoved(Furniture obj)
        {
            World.Current.PowerNetwork.Unplug(this);
            ParentFurniture.Removed -= PowerConnectionRemoved;
        }

        private void OnReconnecting()
        {
            foreach (Utility util in ParentFurniture.Tile.Utilities.Values)
            {
                util.Grid.PlugIn(this);
            }
        }

        [Serializable]
        [JsonObject(MemberSerialization.OptOut)]
        public class Info
        {
            [XmlAttribute("rate")]
            public float Rate { get; set; }

            [XmlAttribute("capacity")]
            public float Capacity { get; set; }

            [XmlAttribute("capacityThresholds")]
            public int CapacityThresholds { get; set; }

            [XmlElement("Param")]
            public List<ParameterCondition> ParamConditions { get; set; }            
        }

        [Serializable]
        [JsonObject(MemberSerialization.OptOut)]
        public class PowerConnectionParameterDefinitions
        {
            // constants for parameters
            public const string CurAccumulatorChargeParamName = "pow_accumulator_charge";
            public const string CurAccumulatorIndexParamName = "pow_accumulator_index";
            public const string CurIsRunningParamName = "pow_is_running";
            
            public PowerConnectionParameterDefinitions()
            {
                // defaults
                CurrentAcumulatorCharge = new ParameterDefinition(CurAccumulatorChargeParamName);
                CurrentAcumulatorChargeIndex = new ParameterDefinition(CurAccumulatorIndexParamName);
                IsRunning = new ParameterDefinition(CurIsRunningParamName);
            }

            public ParameterDefinition CurrentAcumulatorCharge { get; set; }

            public ParameterDefinition CurrentAcumulatorChargeIndex { get; set; }

            public ParameterDefinition IsRunning { get; set; }
        }
    }
}
