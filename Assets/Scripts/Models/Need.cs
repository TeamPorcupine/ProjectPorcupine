#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public class Need 
{
    public string needType;
    public string localisationID;
    public string Name;
    public Character character;
    protected float addedInVacuum;
    protected float dps;
    protected string[] luaUpdate;
    protected bool luaOnly = false;
    protected float restoreNeedAmount = 100;
    protected float growthRate;
    protected bool highToLow = true;
    private float amount = 0;

    // Use this for initialization
    public Need()
    {
        Amount = 0;
    }

    protected Need(Need other)
    {
        Amount = 0;
        this.needType = other.needType;
        this.localisationID = other.localisationID;
        this.Name = other.Name;
        this.growthRate = other.growthRate;
        this.highToLow = other.highToLow;
        this.RestoreNeedFurn = other.RestoreNeedFurn;
        this.RestoreNeedTime = other.RestoreNeedTime;
        this.restoreNeedAmount = other.restoreNeedAmount;
        this.CompleteOnFail = other.CompleteOnFail;
        this.addedInVacuum = other.addedInVacuum;
        this.dps = other.dps;
        this.luaUpdate = other.luaUpdate;
        this.luaOnly = other.luaOnly;
    }

    public float Amount 
    { 
        get 
        {
            return amount;
        }

        set
        {
            float f = value;
            if (f < 0)
            {
                f = 0;
            }

            if (f > 100)
            {
                f = 100;
            }

            amount = f;
        }
    }

    public bool CompleteOnFail { get; protected set; }

    public Furniture RestoreNeedFurn { get; protected set; }

    public float RestoreNeedTime { get; protected set; }

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

    // Update is called once per frame
    public void Update(float deltaTime)
    {
        NeedActions.CallFunctionsWithNeed(luaUpdate, this, deltaTime);
        if (luaOnly)
        {
            return;
        }

        Amount += growthRate * deltaTime;
        if (character != null && character.CurrTile.GetGasPressure("O2") < 0.15)
        {
            Amount += (addedInVacuum - (addedInVacuum * (character.CurrTile.GetGasPressure("O2") * 5))) * deltaTime;
        }

        if (Amount > 75 && character.MyJob.IsNeed == false)
        {
            Debug.ULogChannel("Need", character.name + " needs " + Name);
            character.AbandonJob(false);
        }

        if (Amount == 100 && character.MyJob.Critical == false && CompleteOnFail)
        {
            Debug.ULogChannel("Need", character.name + " failed their " + Name + " need.");
            character.AbandonJob(false);
        }

        if (Amount == 100)
        {
            // FIXME: Insert need fail damage code here.
        }
    }

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        needType = reader_parent.GetAttribute("needType");

        XmlReader reader = reader_parent.ReadSubtree();
        List<string> luaActions = new List<string>();

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
                RestoreNeedFurn = World.Current.furniturePrototypes[reader.ReadContentAsString()];
                break;
            case "RestoreNeedTime":
                reader.Read();
                RestoreNeedTime = reader.ReadContentAsFloat();
                break;
            case "Damage":
                reader.Read();
                dps = reader.ReadContentAsFloat();
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
                growthRate = reader.ReadContentAsFloat();
                break;
            case "GrowthInVacuum":
                reader.Read();
                addedInVacuum = reader.ReadContentAsFloat();
                break;
            case "RestoreNeedAmount":
                reader.Read();
                restoreNeedAmount = reader.ReadContentAsFloat();
                break;
            case "Localization":
                reader.Read();
                localisationID = reader.ReadContentAsString();
                break;
            case "LuaProcessingOnly":
                luaOnly = true;
                break;
            case "OnUpdate":
                reader.Read();
                luaActions.Add(reader.ReadContentAsString());
                break;
            }
        }

        luaUpdate = luaActions.ToArray();
    }

    public void CompleteJobNorm(Job j)
    {
        Amount -= restoreNeedAmount;
    }

    public void CompleteJobCrit(Job j)
    {
        Amount -= restoreNeedAmount / 4;
    }

    public Need Clone()
    {
        return new Need(this);
    }
}
