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
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    public abstract class Component
    {        
        private static Dictionary<string, Type> componentTypes;

        public Component()
        {
            // need to set it, for some reason GetHashCode is called during serialization (when Name is still null)
            Type = string.Empty;
        }
        
        [XmlIgnore]
        public Furniture ParentFurniture { get; set; }
        
        [XmlIgnore]
        public string Type { get; set; }

        public static Component Deserialize(XmlReader xmlReader)
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
                var cmp = (Component)serializer.Deserialize(xmlReader);
                //// need to set name explicitly (not part of deserialization as it's passed in)
                cmp.Type = componentTypeName;
                return cmp;
            }
            else
            {
                Debug.ULogError("There is no deserializer for component '{0}'", componentTypeName);
                return null;
            }
        }

        public virtual void Initialize()
        {
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual List<ComponentContextMenu> GetContextMenu()
        {
            return null;
        }

        public virtual string GetDescription()
        { 
            return null;
        }
        
        public override string ToString()
        {
            return Type;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Type.Equals(obj);
        }

        private static Dictionary<string, System.Type> FindComponentsInAssembly()
        {
            componentTypes = new Dictionary<string, System.Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof(ComponentNameAttribute), false);
                    if (attribs != null && attribs.Length > 0)
                    {
                        foreach (ComponentNameAttribute compNameAttr in attribs)
                        {
                            componentTypes.Add(compNameAttr.ComponentName, type);
                            Debug.ULogChannel("FurnitureComponents", "Found component in assembly: {0}", compNameAttr.ComponentName);
                        }
                    }
                }
            }

            return componentTypes;
        }
    }
}
