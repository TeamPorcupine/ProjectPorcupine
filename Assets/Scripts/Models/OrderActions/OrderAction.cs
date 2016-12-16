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
using System.Xml;
using System.Xml.Serialization;

namespace ProjectPorcupine.OrderActions
{
    [Serializable]
    public abstract class OrderAction
    {
        protected static readonly string OrderActionsLogChannel = "OrderActions";

        private static Dictionary<string, Type> orderActionTypes;

        public OrderAction()
        {
        }

        public OrderAction(OrderAction other)
        {
            Type = other.Type;
        }

        [XmlIgnore]
        public string Type { get; set; }

        public static OrderAction Deserialize(XmlReader xmlReader)
        {
            if (orderActionTypes == null)
            {
                orderActionTypes = FindOrderActionsInAssembly();
            }

            string orderActionType = xmlReader.GetAttribute("type");
            if (orderActionTypes.ContainsKey(orderActionType))
            {
                xmlReader = xmlReader.ReadSubtree();
                Type t = orderActionTypes[orderActionType];
                XmlSerializer serializer = new XmlSerializer(t);
                OrderAction orderAction = (OrderAction)serializer.Deserialize(xmlReader);
                //// need to set name explicitly (not part of deserialization as it's passed in)
                orderAction.Initialize(orderActionType);
                return orderAction;
            }
            else
            {
                UnityDebugger.Debugger.Log(OrderActionsLogChannel, string.Format("There is no deserializer for OrderAction '{0}'", orderActionType));
                return null;
            }
        }

        public virtual void Initialize(string type)
        {
            Type = type;
        }

        public abstract Job CreateJob(Tile tile, string type);

        public override string ToString()
        {
            return Type;
        }

        public abstract OrderAction Clone();

        protected Job CheckJobFromFunction(string functionName, Furniture furniture)
        {
            Job job = null;
            if (!string.IsNullOrEmpty(functionName))
            {
                job = FunctionsManager.Furniture.Call<Job>(functionName, furniture, null);
                job.OrderName = Type;
            }

            return job;
        }

        private static Dictionary<string, System.Type> FindOrderActionsInAssembly()
        {
            var orderActionTypes = new Dictionary<string, System.Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(asm => !CSharpFunctions.IsDynamic(asm)))
            {
                foreach (Type type in assembly.GetTypes())
                {
                    OrderActionNameAttribute[] attribs = (OrderActionNameAttribute[])type.GetCustomAttributes(typeof(OrderActionNameAttribute), false);
                    if (attribs != null && attribs.Length > 0)
                    {
                        foreach (OrderActionNameAttribute compNameAttr in attribs)
                        {
                            orderActionTypes.Add(compNameAttr.OrderActionName, type);
                            UnityDebugger.Debugger.Log(OrderActionsLogChannel, string.Format("Found OrderAction in assembly: {0}", compNameAttr.OrderActionName));
                        }
                    }
                }
            }

            return orderActionTypes;
        }

        [Serializable]
        public class JobInformation
        {
            [XmlAttribute("time")]
            public int Time { get; set; }

            [XmlAttribute("fromFunction")]
            public string FromFunction { get; set; }
        }

        [Serializable]
        public class InventoryInfo
        {
            [XmlAttribute("type")]
            public string Type { get; set; }
            [XmlAttribute("amount")]
            public int Amount { get; set; }
        }
    }
}
