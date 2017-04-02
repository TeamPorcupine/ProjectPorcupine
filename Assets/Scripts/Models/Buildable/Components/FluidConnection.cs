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
    [BuildableComponentName("FluidConnection")]
    [MoonSharpUserData]
    public class FluidConnection : BuildableComponent, IPluggable
    {       
        public FluidConnection()
        {
            SubType = string.Empty;
        }

        private FluidConnection(FluidConnection other) : base(other)
        {
            ParamsDefinitions = other.ParamsDefinitions;
            Provides = other.Provides;
            Requires = other.Requires;
            RunConditions = other.RunConditions;
            SubType = other.SubType;

            Reconnecting += OnReconnecting;
        }

        public event Action Reconnecting;

        [XmlElement("ParameterDefinitions")]
        [JsonProperty("ParameterDefinitions")]
        public FluidConnectionParameterDefinitions ParamsDefinitions { get; set; }

        public Parameter CurrentStoredAmount
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.CurrentStoredAmount.ParameterName];
            }
        }

        public Parameter CurrentStorageIndex
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.CurrentStorageIndex.ParameterName];
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

        [XmlElement("RunConditions")]
        [JsonProperty("RunConditions")]
        public Conditions RunConditions { get; set; }

        [XmlIgnore]
        public float StoredAmount
        {
            get
            {
                return CurrentStoredAmount.ToFloat();
            }

            set
            {
                float oldStoredAmount = CurrentStoredAmount.ToFloat();
                if (oldStoredAmount != value)
                {
                    CurrentStoredAmount.SetValue(value);

                    int curIndex = (int)((Provides.CapacityThresholds - 1) * (value / Provides.Capacity));
                    CurrentStorageIndex.SetValue(curIndex);
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
                return "Fluid";
            }
        }

        [XmlElement("FluidType")]
        [JsonProperty("FluidType")]
        public string SubType { get; set; }

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
            get { return false; }
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
            get
            {
                // TODO: implement for fluids
                return false;
            }
        }

        public bool OutputIsNeeded { get; set; }
       
        public bool AllRequirementsFulfilled
        {
            get
            {
                // TODO: implement for fluids
                return true;
            }
        }

        public override BuildableComponent Clone()
        {
            return new FluidConnection(this);
        }

        public override bool CanFunction()
        {
            bool hasPower = true;
            if (IsConsumer)
            {
                hasPower = World.Current.FluidNetwork.HasPower(this);
            }

            return hasPower;
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
        }

        public override IEnumerable<string> GetDescription()
        {           
            string powerColor = IsRunning ? "lime" : "red";
            string status = IsRunning ? "online" : "offline";
            yield return LocalizationTable.GetLocalization("fluid_grid_status_" + status, powerColor);

            // Debugging info, should be removed
            yield return "Fluid Type: " + SubType;

            if (IsConsumer && Requires != null)
            {
                yield return LocalizationTable.GetLocalization("fluid_input_status", powerColor, Requires.Rate);
            }

            if (IsProducer && Requires != null)
            {
                yield return LocalizationTable.GetLocalization("fluid_output_status", powerColor, Requires.Rate);
            }

            if (IsStorage && Provides != null)
            {
                yield return LocalizationTable.GetLocalization("fluid_accumulated_fraction", StoredAmount, Provides.Capacity);
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
            componentRequirements = Requirements.Fluid;

            if (ParamsDefinitions == null)
            {
                // don't need definition for all furniture, just use defaults
                ParamsDefinitions = new FluidConnectionParameterDefinitions();
            }

            // need to keep accumulator current charge in parameters

            // TODO: need to prevent defining same type more than once?
            if (Provides != null)
            {
                if (Provides.Capacity > 0)
                {
                    CurrentStoredAmount.SetValue(0f);
                    CurrentStorageIndex.SetValue(0);
                }
            }

            IsRunning = false;

            OnReconnecting();

            ParentFurniture.Removed += FluidConnectionRemoved;           
        }

        private void FluidConnectionRemoved(Furniture obj)
        {
            World.Current.FluidNetwork.Unplug(this);
            ParentFurniture.Removed -= FluidConnectionRemoved;
        }

        private void OnReconnecting()
        {
            // TODO: Make this not a Universal Connection
            World.Current.FluidNetwork.PlugIn(this);
        }
        
        [Serializable]
        [JsonObject(MemberSerialization.OptOut)]
        public class FluidConnectionParameterDefinitions
        {
            // constants for parameters
            public const string CurStoredAmountParamName = "fluid_stored_amount";
            public const string CurStorageIndexParamName = "fluid_storage_index";
            public const string CurIsRunningParamName = "fluid_is_running";

            public FluidConnectionParameterDefinitions()
            {
                // defaults
                CurrentStoredAmount = new ParameterDefinition(CurStoredAmountParamName);
                CurrentStorageIndex = new ParameterDefinition(CurStorageIndexParamName);
                IsRunning = new ParameterDefinition(CurIsRunningParamName);
            }

            public ParameterDefinition CurrentStoredAmount { get; set; }

            public ParameterDefinition CurrentStorageIndex { get; set; }

            public ParameterDefinition IsRunning { get; set; }
        }
    }
}
