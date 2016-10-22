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

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    [XmlRoot("Component")]
    [BuildableComponentName("Animator")]
    public class Animator : BuildableComponent
    {
        public Animator()
        {
        }

        private Animator(Animator other) : base(other)
        {
            UsedAnimations = other.UsedAnimations;
        }

        [XmlElement("UseAnimation")]
        public List<UseAnimation> UsedAnimations { get; set; }
        
        [XmlIgnore]
        public string CurrentAnimationName { get; private set; }

        [XmlIgnore]
        private string DefaultAnimationName { get; set; }

        public override BuildableComponent Clone()
        {
            return new Animator(this);
        }

        public override void FixedFrequencyUpdate(float deltaTime)
        {
            if (UsedAnimations != null && ParentFurniture.Animation != null && UsedAnimations.Count > 0)
            {
                foreach (var anim in UsedAnimations)
                {
                    if (!string.IsNullOrEmpty(anim.ValueBasedParamerName))
                    {
                        // is value based animation
                        if (ParentFurniture.Animation != null)
                        {
                            int frmIdx = FurnitureParams[anim.ValueBasedParamerName].ToInt();
                            ParentFurniture.Animation.SetFrameIndex(frmIdx);
                        }
                    }
                    else if (anim.Requires.ParamConditions != null)
                    {
                        if (AreParameterConditionsFulfilled(anim.Requires.ParamConditions))
                        {
                            ChangeAnimation(anim.Name);
                            break;
                        }                        
                    }
                }
            }            
        }

        protected override void Initialize()
        {
            if (UsedAnimations != null && ParentFurniture.Animation != null && UsedAnimations.Count > 0)
            {
                ParentFurniture.Animation.SetState(UsedAnimations[0].Name);
                DefaultAnimationName = CurrentAnimationName = UsedAnimations[0].Name;
            }

            ParentFurniture.IsOperatingChanged += (furniture) => SetDefaultAnimation(furniture.IsOperating);
        }

        private void ChangeAnimation(string newAnimation)
        {
            if (newAnimation != CurrentAnimationName && ParentFurniture.Animation != null)
            {
                ParentFurniture.Animation.SetState(newAnimation);
                CurrentAnimationName = newAnimation;
            }
        }

        private void SetDefaultAnimation(bool setDefault)
        {
            if (setDefault)
            {
                ChangeAnimation(DefaultAnimationName);
            }
        }       
    }
}
