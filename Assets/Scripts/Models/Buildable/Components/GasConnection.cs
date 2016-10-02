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
using ProjectPorcupine.Rooms;

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    [XmlRoot("Component")]
    [BuildableComponentName("GasConnection")]
    public class GasConnection : BuildableComponent
    {
        private event Action<bool> OnRunningStateChanged;

        [XmlElement("Provides")]
        public List<GasInfo> Provides { get; set; }

        [XmlElement("Requires")]
        public List<GasInfo> Requires { get; set; }

        [XmlElement("UsedAnimations")]
        public UsedAnimations UsedAnimation { get; set; }

        [XmlIgnore]
        public bool IsRunning { get; private set; }
        
        public override bool CanFunction()
        {
            bool canFunction = true;
            //// check if all requirements are fullfilled
            if (Requires != null && Requires.Count > 0)
            {
                Room room = ParentFurniture.Tile.Room;
                foreach (GasInfo reqGas in Requires)
                {
                    // get current gas rounded so it is in sync with UI
                    float curGasPressure = (float)Math.Round((decimal)room.GetGasPressure(reqGas.Gas), 3);
                    if (curGasPressure < reqGas.MinLimit || curGasPressure > reqGas.MaxLimit)
                    {
                        canFunction &= false;
                    }
                }
            }

            return canFunction;
        }

        public override void FixedFrequencyUpdate(float deltaTime)
        {
            bool isWorking = false;
            if (Provides != null && Provides.Count > 0)
            {
                Room room = ParentFurniture.Tile.Room;
                foreach (GasInfo provGas in Provides)
                {
                    // get current gas rounded so it is in sync with UI
                    float curGasPressure = (float)Math.Round((decimal)room.GetGasPressure(provGas.Gas), 3);
                    if ((provGas.Rate > 0 && curGasPressure < provGas.MaxLimit) ||
                        (provGas.Rate < 0 && curGasPressure > provGas.MinLimit))
                    {
                        room.ChangeGas(provGas.Gas, provGas.Rate * deltaTime, provGas.MaxLimit);
                        isWorking |= true;
                    }                    
                }
            }

            if (isWorking)
            {
                //// trigger running state change
                if (!IsRunning)
                {
                    OnRunningStateChanged(IsRunning = true);
                }
            }
            else
            {
                //// trigger running state change
                if (IsRunning)
                {
                    OnRunningStateChanged(IsRunning = false);
                }
            }
        }

        protected override void Initialize()
        {
            IsRunning = false;
            OnRunningStateChanged += RunningStateChanged;
        }

        private void RunningStateChanged(bool newIsRunningState)
        {
            if (UsedAnimation != null)
            {
                if (newIsRunningState == true && !string.IsNullOrEmpty(UsedAnimation.Running))
                {
                    ParentFurniture.Animation.SetState(UsedAnimation.Running);
                }
                else if (newIsRunningState == false && !string.IsNullOrEmpty(UsedAnimation.Idle))
                {
                    ParentFurniture.Animation.SetState(UsedAnimation.Idle);
                }
            }
        }

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
