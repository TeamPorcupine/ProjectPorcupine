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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    public abstract class BuildableComponent
    {
        protected static readonly string ComponentLogChannel = "FurnitureComponents";

        private static Dictionary<string, Type> componentTypes;

        public BuildableComponent()
        {
            // need to set it, for some reason GetHashCode is called during serialization (when Name is still null)
            Type = string.Empty;
        }

        public BuildableComponent(BuildableComponent other)
        {
            Type = other.Type;
        }

        public enum ConditionType
        {
            IsGreaterThanZero,
            IsZero,
            IsTrue,
            IsFalse
        }

        [XmlIgnore]
        public string Type { get; set; }

        [XmlIgnore]
        protected Furniture ParentFurniture { get; set; }

        [XmlIgnore]
        protected Parameter FurnitureParams
        {
            get { return ParentFurniture.Parameters; }
        }
                
        public static BuildableComponent Deserialize(XmlReader xmlReader)
        {
            if (componentTypes == null)
            {
                componentTypes = FindComponentsInAssembly();
            }

            string componentTypeName = xmlReader.GetAttribute("type");
            if (componentTypes.ContainsKey(componentTypeName))
            {
                xmlReader = xmlReader.ReadSubtree();
                Type t = componentTypes[componentTypeName];
                XmlSerializer serializer = new XmlSerializer(t);
                var cmp = (BuildableComponent)serializer.Deserialize(xmlReader);
                //// need to set name explicitly (not part of deserialization as it's passed in)
                cmp.Type = componentTypeName;
                return cmp;
            }
            else
            {
                Debug.ULogErrorChannel(ComponentLogChannel, "There is no deserializer for component '{0}'", componentTypeName);
                return null;
            }
        }
        
        public void Initialize(Furniture parentFurniture)
        {
            ParentFurniture = parentFurniture;
            Initialize();
        }

        public virtual bool CanFunction()
        {
            return true;
        }

        public virtual void FixedFrequencyUpdate(float deltaTime)
        {
        }

        public virtual void EveryFrameUpdate(float deltaTime)
        {
        }

        public virtual List<ContextMenuAction> GetContextMenu()
        {
            return null;
        }

        public virtual IEnumerable<string> GetDescription()
        { 
            return null;
        }
        
        public override string ToString()
        {
            return Type;
        }
        
        public abstract BuildableComponent Clone();
        
        protected abstract void Initialize();

        protected ContextMenuAction CreateComponentContextMenuItem(ComponentContextMenu componentContextMenuAction)
        {
            return new ContextMenuAction
            {
                Text = componentContextMenuAction.Name, // TODO: localization here
                RequireCharacterSelected = false,
                Action = (cma, c) => InvokeContextMenuAction(componentContextMenuAction.Function, componentContextMenuAction.Name)
            };
        }

        protected void InvokeContextMenuAction(Action<Furniture, string> function, string arg)
        {
            function(ParentFurniture, arg);
        }
        
        protected bool AreParameterConditionsFulfilled(List<ParameterCondition> conditions)
        {
            bool conditionsFulFilled = true;
            //// here evaluate all parameter conditions
            if (conditions != null)
            {
                foreach (ParameterCondition cnd in conditions)
                {
                    bool partialEval = true;
                    switch (cnd.Condition)
                    {
                        case ConditionType.IsZero:
                            partialEval = FurnitureParams[cnd.ParameterName].ToFloat().Equals(0);
                            break;
                        case ConditionType.IsGreaterThanZero:
                            partialEval = FurnitureParams[cnd.ParameterName].ToFloat() > 0f;
                            break;
                        case ConditionType.IsTrue:
                            partialEval = FurnitureParams[cnd.ParameterName].ToBool() == true;
                            break;
                        case ConditionType.IsFalse:
                            partialEval = FurnitureParams[cnd.ParameterName].ToBool() == false;
                            break;
                    }

                    conditionsFulFilled &= partialEval;
                }
            }

            return conditionsFulFilled;
        }

        private static Dictionary<string, System.Type> FindComponentsInAssembly()
        {
            componentTypes = new Dictionary<string, System.Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof(BuildableComponentNameAttribute), false);
                    if (attribs != null && attribs.Length > 0)
                    {
                        foreach (BuildableComponentNameAttribute compNameAttr in attribs)
                        {
                            componentTypes.Add(compNameAttr.ComponentName, type);
                            Debug.ULogChannel(ComponentLogChannel, "Found component in assembly: {0}", compNameAttr.ComponentName);
                        }
                    }
                }
            }

            return componentTypes;
        }

        [Serializable]
        public class UseAnimation
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
                        
            [XmlAttribute("valuebasedParamerName")]
            public string ValueBasedParamerName { get; set; }
            
            public ParameterConditions Requires { get; set; }
        }
        
        [Serializable]
        public class ParameterConditions
        {
            [XmlElement("Param")]
            public List<ParameterCondition> ParamConditions { get; set; }
        }

        [Serializable]
        public class ParameterCondition
        {
            [XmlAttribute("name")]
            public string ParameterName { get; set; }

            [XmlAttribute("condition")]
            public ConditionType Condition { get; set; }
        }

        [Serializable]
        public class ParameterDefinition
        {              
            public ParameterDefinition()
            {
            }

            public ParameterDefinition(string paramName)
            {
                this.ParameterName = paramName;
            }

            [XmlAttribute("name")]
            public string ParameterName { get; set; }

            [XmlAttribute("type")]
            public string Type { get; set; }
        }
    }    
}
