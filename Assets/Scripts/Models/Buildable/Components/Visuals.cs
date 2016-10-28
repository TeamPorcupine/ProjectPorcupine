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

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
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
        public SpriteNameInfo DefaultSpriteName { get; set; }

        [XmlElement("SpriteName")]
        public SpriteNameInfo SpriteName { get; set; }

        [XmlElement("OverlaySpriteName")]
        public SpriteNameInfo OverlaySpriteName { get; set; }

        [XmlElement("UseAnimation")]
        public List<UseAnimation> UsedAnimations { get; set; }
        
        [XmlIgnore]
        public string CurrentAnimationName { get; private set; }

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

        public override void InitializePrototype(Furniture protoFurniture)
        {
            // default sprite (used for showing sprite in menu)
            protoFurniture.DefaultSpriteName = RetrieveSpriteNameFor(DefaultSpriteName, protoFurniture);
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
            ParentFurniture.SpriteName = RetrieveSpriteNameFor(SpriteName, ParentFurniture);

            // overlay sprite, if any
            ParentFurniture.OverlaySpriteName = RetrieveSpriteNameFor(OverlaySpriteName, ParentFurniture);
        }

        private string RetrieveSpriteNameFor(SpriteNameInfo spriteNameInfo, Furniture furniture)
        {
            string useSpriteName = null;
            if (spriteNameInfo != null)
            {
                if (!string.IsNullOrEmpty(spriteNameInfo.UseName))
                {
                    useSpriteName = spriteNameInfo.UseName;
                }
                else if (!string.IsNullOrEmpty(spriteNameInfo.FromFunction))
                {
                    DynValue ret = FunctionsManager.Furniture.Call(spriteNameInfo.FromFunction, furniture);
                    useSpriteName = ret.String;
                }
                else if (!string.IsNullOrEmpty(spriteNameInfo.FromParameter))
                {
                    useSpriteName = furniture.Parameters[spriteNameInfo.FromParameter].ToString();
                }                
            }

            return useSpriteName;
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

        [Serializable]
        public class SpriteNameInfo
        {
            [XmlAttribute("useName")]
            public string UseName { get; set; }

            [XmlAttribute("fromParameter")]
            public string FromParameter { get; set; }

            [XmlAttribute("fromFunction")]
            public string FromFunction { get; set; }
        }
    }
}
