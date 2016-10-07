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
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    [XmlRoot("Component")]
    [BuildableComponentName("PowerConnection")]
    public class PowerConnection : BuildableComponent
    {
        // constants for parameters
        public const string CurAccumulatorChargeParamName = "pow_accumulator_charge";
        public const string CurAccumulatorIndexParamName = "pow_accumulator_index";
        public const string CurIsRunningParamName = "pow_is_running";
        
        public PowerConnection()
        {
        }

        private PowerConnection(PowerConnection other) : base(other)
        {
            Provides = other.Provides;
            Requires = other.Requires;
        }
              
        [XmlIgnore]
        public bool IsRunning
        {
            get
            {
                bool running = false;

                // TODO: make adjustments in Parameters class to prevent double check for ContainsKey (indexer already checks it)
                if (FurnitureParams.ContainsKey(CurIsRunningParamName))
                {
                    running = FurnitureParams[CurIsRunningParamName].ToBool();
                }                

                return running;
            }

            set
            {
                FurnitureParams[CurIsRunningParamName].SetValue(value);
            }
        }

        [XmlElement("Provides")]
        public Info Provides { get; set; }

        [XmlElement("Requires")]
        public Info Requires { get; set; }
        
        [XmlIgnore]
        public float AccumulatedPower
        {
            get
            {
                return FurnitureParams != null ? FurnitureParams[CurAccumulatorChargeParamName].ToFloat() : 0f;
            }

            set
            {
                float oldAccPower = FurnitureParams[CurAccumulatorChargeParamName].ToFloat();
                if (oldAccPower != value)
                {
                    FurnitureParams[CurAccumulatorChargeParamName].SetValue(value);

                    int curIndex = (int)((Provides.CapacityThresholds - 1) * (value / Provides.Capacity));
                    FurnitureParams[CurAccumulatorIndexParamName].SetValue(curIndex);
                }
            }
        }

        public bool IsEmpty
        {
            get { return IsPowerAccumulator && AccumulatedPower.IsZero(); }
        }
        
        public bool IsFull
        {
            get { return IsPowerAccumulator && AccumulatedPower.AreEqual(Provides.Capacity); }
        }

        public bool IsPowerProducer
        {
            get { return Provides != null && IsRunning && Provides.Rate > 0.0f && !IsPowerAccumulator; }
        }

        public bool IsPowerConsumer
        {
            get { return Requires != null && Requires.Rate > 0.0f && !IsPowerAccumulator; }
        }

        public bool IsPowerAccumulator
        {
            get { return Provides != null && Provides.Capacity > 0.0f; }
        }

        public override BuildableComponent Clone()
        {
            return new PowerConnection(this);
        }

        public override bool CanFunction()
        {
            bool hasPower = true;
            if (IsPowerConsumer)
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

            if (IsPowerAccumulator)
            {
                areAllParamReqsFulfilled &= AccumulatedPower > 0;
            }

            IsRunning = areAllParamReqsFulfilled;
        }
        
        public override IEnumerable<string> GetDescription()
        {           
            string powerColor = IsRunning ? "lime" : "red";

            yield return string.Format("Power Grid: <color={0}>{1}</color>", powerColor, IsRunning ? "Online" : "Offline");

            if (IsPowerConsumer)
            {
                yield return string.Format("Power Input: <color={0}>{1}</color>", powerColor, Requires.Rate);
            }

            if (IsPowerProducer)
            {
                yield return string.Format("Power Output: <color={0}>{1}</color>", powerColor, Provides.Rate);
            }

            if (IsPowerAccumulator)
            {
                yield return string.Format("Power Accumulated: {0} / {1}", AccumulatedPower, Provides.Capacity);
            }
        }

        protected override void Initialize()
        {
            // need to keep accumulator current charge in parameters
            // TODO: need to prevent defining same type more than once
            if (Provides != null)
            {
                if (Provides.Capacity > 0)
                {
                    FurnitureParams.AddParameter(new Parameter(CurAccumulatorChargeParamName, 0f));
                    FurnitureParams.AddParameter(new Parameter(CurAccumulatorIndexParamName, 0));
                }
            }

            FurnitureParams.AddParameter(new Parameter(CurIsRunningParamName, false));

            IsRunning = false;

            World.Current.PowerNetwork.PlugIn(this);
            ParentFurniture.Removed += PowerConnectionRemoved;           
        }
        
        private void PowerConnectionRemoved(Furniture obj)
        {
            World.Current.PowerNetwork.Unplug(this);
            ParentFurniture.Removed -= PowerConnectionRemoved;
        }

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
    }
}