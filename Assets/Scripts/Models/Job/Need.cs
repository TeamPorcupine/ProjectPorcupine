#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Xml;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class Need : IPrototypable
{
    private bool highToLow = true;
    private float amount = 0;

    // Use this for initialization
    public Need()
    {
        Amount = 0;
        RestoreNeedAmount = 100;
        EventActions = new EventActions();
    }

    private Need(Need other)
    {
        Amount = 0;
        Type = other.Type;
        LocalizationID = other.LocalizationID;
        Name = other.Name;
        GrowthRate = other.GrowthRate;
        highToLow = other.highToLow;
        RestoreNeedFurn = other.RestoreNeedFurn;
        RestoreNeedTime = other.RestoreNeedTime;
        RestoreNeedAmount = other.RestoreNeedAmount;
        Damage = other.Damage;

        if (other.EventActions != null)
        {
            EventActions = other.EventActions.Clone();
        }
    }

    public Character Character { get; set; }

    public string Type { get; private set; }

    public string LocalizationID { get; private set; }

    public string Name { get; private set; }

    public float Amount
    {
        get
        {
            return amount;
        }

        set
        {
            amount = value.Clamp(0.0f, 100.0f);
        }
    }

    public float RestoreNeedAmount { get; private set; }

    public float GrowthRate { get; private set; }

    public float Damage { get; private set; }

    public bool CompleteOnFail { get; private set; }

    public Furniture RestoreNeedFurn { get; private set; }

    public float RestoreNeedTime { get; private set; }

    public string DisplayAmount
    {
        get
        {
            if (highToLow)
            {
                return (100 - (int)Amount) + "%";
            }

            return ((int)Amount) + "%";
        }
    }

    /// <summary>
    /// Gets the EventAction for the current furniture.
    /// These actions are called when an event is called. They get passed the furniture
    /// they belong to, plus a deltaTime (which defaults to 0).
    /// </summary>
    /// <value>The event actions that is called on update.</value>
    public EventActions EventActions { get; private set; }

    // Update is called once per frame
    public void Update(float deltaTime)
    {
        if (EventActions != null && EventActions.HasEvent("OnUpdate"))
        {
            EventActions.Trigger("OnUpdate", this, deltaTime);
        }
        else
        {
            DefaultNeedDecay(deltaTime);
        }

        if (Amount.AreEqual(100))
        {
            if (EventActions != null && EventActions.HasEvent("OnEmptyNeed"))
            {
                EventActions.Trigger("OnEmptyNeed", this, deltaTime);
            }
            else
            {
                DefaultEmptyNeed();
            }
        }
        else if (Amount > 90f)
        {
            if (EventActions != null)
            {
                EventActions.Trigger("OnSevereNeed", this, deltaTime);
            }
        }
        else if (Amount > 75f)
        {
            if (EventActions != null)
            {
                EventActions.Trigger("OnCriticalNeed", this, deltaTime);
            }
        }
        else if (Amount > 50f)
        {
            if (EventActions != null)
            {
                EventActions.Trigger("OnModerateNeed", this, deltaTime);
            }
        }
    }

    public void ReadXmlPrototype(XmlReader parentReader)
    {
        Type = parentReader.GetAttribute("type");

        XmlReader reader = parentReader.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Name":
                    reader.Read();
                    Name = reader.ReadContentAsString();
                    break;
                case "RestoreNeedFurnitureType":
                    reader.Read();
                    RestoreNeedFurn = PrototypeManager.Furniture.Get(reader.ReadContentAsString());
                    break;
                case "RestoreNeedTime":
                    reader.Read();
                    RestoreNeedTime = reader.ReadContentAsFloat();
                    break;
                case "Damage":
                    reader.Read();
                    Damage = reader.ReadContentAsFloat();
                    break;
                case "CompleteOnFail":
                    reader.Read();
                    CompleteOnFail = reader.ReadContentAsBoolean();
                    break;
                case "HighToLow":
                    reader.Read();
                    highToLow = reader.ReadContentAsBoolean();
                    break;
                case "GrowthRate":
                    reader.Read();
                    GrowthRate = reader.ReadContentAsFloat();
                    break;
                case "RestoreNeedAmount":
                    reader.Read();
                    RestoreNeedAmount = reader.ReadContentAsFloat();
                    break;
                case "Localization":
                    reader.Read();
                    LocalizationID = reader.ReadContentAsString();
                    break;
                case "Action":
                    XmlReader subtree = reader.ReadSubtree();
                    EventActions.ReadXml(subtree);
                    subtree.Close();
                    break;
            }
        }
    }

    public void CompleteJobNorm(Job job)
    {
        Amount -= RestoreNeedAmount;
    }

    public void CompleteJobCrit(Job job)
    {
        Amount -= RestoreNeedAmount / 4;
    }

    public Need Clone()
    {
        return new Need(this);
    }

    public void DefaultNeedDecay(float deltaTime)
    {
        Amount += this.GrowthRate * deltaTime;
    }

    public void DefaultEmptyNeed()
    {
        // TODO: Default for empty need should probably be taking damage, but shouldn't be implemented until characters are 
        //       better able to handle getting their oxygen and maybe have real space suits.
    }
}
