using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    public abstract class Component
    {
        // TODO: use ComponentNameAttribute and fetch names from assembly
        static Dictionary<string, Type> componentTypes = new Dictionary<string, Type>
        {
            { "Workshop", typeof(Workshop) }
        };

        public Component()
        {
            // need to set it, for some reason GetHashCode is called during serialization (when Name is still null)
            Type = string.Empty;
        }
        
        [XmlIgnore]
        public Furniture ParentFurniture { get; set; }
        
        [XmlIgnore]
        public string Type { get; set; }

        public virtual void Initialize() { }
        public virtual void Update(float deltaTime) { }

        public virtual List<ComponentContextMenu> GetContextMenu() { return null; }

        public virtual string GetDescription()
        { 
            return null;
        }
               
        public static Component Deserialize(XmlReader xmlReader)
        {
            string componentTypeName = xmlReader.GetAttribute("type");
            if (componentTypes.ContainsKey(componentTypeName))
            {
                xmlReader = xmlReader.ReadSubtree();
                Type t = componentTypes[componentTypeName];
                XmlSerializer serializer = new XmlSerializer(t, componentTypes.Values.ToArray());
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
    }
}
