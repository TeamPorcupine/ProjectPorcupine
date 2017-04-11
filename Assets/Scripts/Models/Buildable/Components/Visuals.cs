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

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    [XmlRoot("Component")]
    [BuildableComponentName("Visuals")]
    public class Visuals : BuildableComponent
    {
        public Visuals()
        {
        }

        private Visuals(Visuals other) : base(other)
        {
            SpriteName = other.SpriteName;
            DefaultSpriteName = other.DefaultSpriteName;
            OverlaySpriteName = other.OverlaySpriteName;
            UsedAnimations = other.UsedAnimations;
        }

        [XmlElement("DefaultSpriteName")]
        [JsonProperty("DefaultSpriteName")]
        public SourceDataInfo DefaultSpriteName { get; set; }

        [XmlElement("SpriteName")]
        [JsonProperty("SpriteName")]
        public SourceDataInfo SpriteName { get; set; }

        [XmlElement("OverlaySpriteName")]
        [JsonProperty("OverlaySpriteName")]
        public SourceDataInfo OverlaySpriteName { get; set; }

        [XmlElement("UseAnimation")]
        [JsonProperty("UseAnimation")]
        public List<UseAnimation> UsedAnimations { get; set; }
        
        [XmlIgnore]
        public string CurrentAnimationName { get; private set; }

        public override bool RequiresSlowUpdate
        {
            get
            {
                return Initialized && (UsedAnimations != null && ParentFurniture.Animation != null && UsedAnimations.Count > 0);
            }
        }

        [XmlIgnore]
        private string DefaultAnimationName { get; set; }

        public override BuildableComponent Clone()
        {
            return new Visuals(this);
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
                    else if (anim.RunConditions.ParamConditions != null)
                    {
                        if (AreParameterConditionsFulfilled(anim.RunConditions.ParamConditions))
                        {
                            ChangeAnimation(anim.Name);
                            break;
                        }                        
                    }
                }
            }            
        }

        public override void InitializePrototype(Furniture protoFurniture)
        {
            // default sprite (used for showing sprite in menu)
            protoFurniture.DefaultSpriteName = RetrieveStringFor(DefaultSpriteName, protoFurniture);
        }

        protected override void Initialize()
        {
            if (UsedAnimations != null && ParentFurniture.Animation != null && UsedAnimations.Count > 0)
            {
                ParentFurniture.Animation.SetState(UsedAnimations[0].Name);
                DefaultAnimationName = CurrentAnimationName = UsedAnimations[0].Name;
            }
            
            ParentFurniture.Changed += FurnitureChanged;
            ParentFurniture.IsOperatingChanged += (furniture) => SetDefaultAnimation(furniture.IsOperating);
        }

        private void FurnitureChanged(Furniture obj)
        {
            // regular sprite
            ParentFurniture.SpriteName = RetrieveStringFor(SpriteName, ParentFurniture);

            // overlay sprite, if any
            ParentFurniture.OverlaySpriteName = RetrieveStringFor(OverlaySpriteName, ParentFurniture);
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
