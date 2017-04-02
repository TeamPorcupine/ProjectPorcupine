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
using System.Linq;
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
            RunConditions = other.RunConditions;

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

        [XmlIgnore]
        public bool IsConnected
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.IsConnected.ParameterName].ToBool();
            }

            set
            {
                FurnitureParams[ParamsDefinitions.IsConnected.ParameterName].SetValue(value);
            }
        }

        [XmlIgnore]
        public float Efficiency
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.Efficiency.ParameterName].ToFloat();
            }

            set
            {
                FurnitureParams[ParamsDefinitions.Efficiency.ParameterName].SetValue(value);
            }
        }

        [XmlIgnore]
        public bool OutputIsNeeded
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.OutputNeeded.ParameterName].ToBool();
            }

            set
            {
                FurnitureParams[ParamsDefinitions.OutputNeeded.ParameterName].SetValue(value);
            }
        }

        [XmlElement("Provides")]
        [JsonProperty("Provides")]
        public Info Provides { get; set; }

        [XmlElement("Requires")]
        [JsonProperty("Requires")]
        public Info Requires { get; set; }

        [XmlElement("RunConditions")]
        [JsonProperty("RunConditions")]
        public Conditions RunConditions { get; set; }
        
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

        public bool InputCanVary
        {
            get { return Requires != null && Requires.CanUseVariableEfficiency == true; }
        }

        public override bool RequiresSlowUpdate
        {
            get
            {
                return true;
            }
        }

        public bool OutputCanVary
        {
            get { return Provides != null && Provides.CanUseVariableEfficiency == true; }
        }

        public bool AllRequirementsFulfilled
        {
            get
            {                
                if (RunConditions != null)
                {
                    return AreParameterConditionsFulfilled(RunConditions.ParamConditions);
                }
                else
                {
                    return true;
                }
            }
        }
        
        public override BuildableComponent Clone()
        {
            return new PowerConnection(this);
        }

        public override bool CanFunction()
        {
            bool canFunction = true;

            if (IsConsumer)
            {
                if (InputCanVary)
                {
                    canFunction = World.Current.PowerNetwork.GetEfficiency(this) > 0f;
                }
                else
                {
                    canFunction = World.Current.PowerNetwork.HasPower(this);
                }
            }

            IsRunning = canFunction;

            bool isConnected = World.Current.PowerNetwork.IsConnected(this);
            IsConnected = isConnected;

            canFunction |= isConnected;

            return canFunction;
        }

        public override void FixedFrequencyUpdate(float deltaTime)
        {
            bool areAllParamReqsFulfilled = true;
            if (RunConditions != null)
            {
                areAllParamReqsFulfilled = AreParameterConditionsFulfilled(RunConditions.ParamConditions);
            }

            if (IsStorage)
            {
                areAllParamReqsFulfilled &= StoredAmount > 0;
            }

            IsRunning = areAllParamReqsFulfilled;

            // store the actual efficiency for other components to use
            if (IsConsumer && Requires.CanUseVariableEfficiency)
            {
                Efficiency = World.Current.PowerNetwork.GetEfficiency(this);                
            }            
        }
        
        public override IEnumerable<string> GetDescription()
        {           
            string powerColor = IsConnected ? "lime" : "red";
            string status = IsConnected ? "connected" : "not connected";
            yield return string.Format("Power grid: <color={0}>{1}</color>", powerColor, status); // TODO: localization 

            if (IsConsumer)
            {
                yield return LocalizationTable.GetLocalization("power_input_status", powerColor, Requires.Rate);

                if (Requires.CanUseVariableEfficiency && IsRunning)
                {
                    yield return string.Format("Electricity: {0:0}%", Efficiency * 100); // TODO: localization
                }
            }

            if (IsProducer)
            {
                if (OutputCanVary)
                {
                    status = OutputIsNeeded ? "demand" : "idle";
                    yield return string.Format("Status: {0}", status); // TODO: localization
                }
                else
                {
                    yield return LocalizationTable.GetLocalization("power_output_status", powerColor, Provides.Rate);
                }
            }

            if (IsStorage)
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

            if (OutputCanVary)
            {
                OutputIsNeeded = false;
            }

            IsRunning = false;
            Efficiency = 1f;

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
            foreach (Utility util in ParentFurniture.GetAllTiles().SelectMany(tile => tile.Utilities.Values))
            {
                if (util.Grid.PlugIn(this))
                {
                    // For now it's meaningless to connect to multiple utilities, and behavior isn't well defined, so bail
                    break;
                }
            }
        }

        [Serializable]
        [JsonObject(MemberSerialization.OptOut)]
        public class PowerConnectionParameterDefinitions
        {
            // constants for parameters
            public const string CurAccumulatorChargeParamName = "pow_accumulator_charge";
            public const string CurAccumulatorIndexParamName = "pow_accumulator_index";
            public const string CurIsRunningParamName = "pow_is_running";
            public const string CurIsConnectedParamName = "pow_is_connected";
            public const string CurEfficiencyParamName = "pow_efficiency";
            public const string CurPowerOutputNeededParamName = "pow_out_needed";

            public PowerConnectionParameterDefinitions()
            {
                // defaults
                CurrentAcumulatorCharge = new ParameterDefinition(CurAccumulatorChargeParamName);
                CurrentAcumulatorChargeIndex = new ParameterDefinition(CurAccumulatorIndexParamName);
                IsRunning = new ParameterDefinition(CurIsRunningParamName);
                IsConnected = new ParameterDefinition(CurIsConnectedParamName);
                Efficiency = new ParameterDefinition(CurEfficiencyParamName);
                OutputNeeded = new ParameterDefinition(CurPowerOutputNeededParamName);
            }

            public ParameterDefinition CurrentAcumulatorCharge { get; set; }

            public ParameterDefinition CurrentAcumulatorChargeIndex { get; set; }

            public ParameterDefinition IsRunning { get; set; }

            public ParameterDefinition IsConnected { get; set; }

            public ParameterDefinition Efficiency { get; set; }

            public ParameterDefinition OutputNeeded { get; set; }
        }
    }
}
