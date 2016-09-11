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

public class Need
{
    public Character character;
    private float addedInVacuum;
    private float dps;
    private string[] luaUpdate;
    private bool luaOnly = false;
    private float restoreNeedAmount = 100;
    private float growthRate;
    private bool highToLow = true;
    private float amount = 0;

    // Use this for initialization
    public Need()
    {
        Amount = 0;
    }

    private Need(Need other)
    {
        Amount = 0;
        Type = other.Type;
        LocalisationID = other.LocalisationID;
        Name = other.Name;
        growthRate = other.growthRate;
        highToLow = other.highToLow;
        RestoreNeedFurn = other.RestoreNeedFurn;
        RestoreNeedTime = other.RestoreNeedTime;
        restoreNeedAmount = other.restoreNeedAmount;
        CompleteOnFail = other.CompleteOnFail;
        addedInVacuum = other.addedInVacuum;
        dps = other.dps;
        luaUpdate = other.luaUpdate;
        luaOnly = other.luaOnly;
    }

    public string Type { get; private set; }

    public string LocalisationID { get; private set; }

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

    // Update is called once per frame
    public void Update(float deltaTime)
    {
        FunctionsManager.Need.CallWithInstance(luaUpdate, this, deltaTime);
        if (luaOnly)
        {
            return;
        }

        Amount += growthRate * deltaTime;

        if (character != null && character.CurrTile.GetGasPressure("O2") < 0.15)
        {
            Amount += (addedInVacuum - (addedInVacuum * (character.CurrTile.GetGasPressure("O2") * 5))) * deltaTime;
        }

        if (Amount.AreEqual(100))
        {
            // FIXME: Insert need fail damage code here.
        }
    }

    public void ReadXmlPrototype(XmlReader parentReader)
    {
        Type = parentReader.GetAttribute("type");

        XmlReader reader = parentReader.ReadSubtree();
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
                    RestoreNeedFurn = PrototypeManager.Furniture.Get(reader.ReadContentAsString());
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
                    LocalisationID = reader.ReadContentAsString();
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

    public void CompleteJobNorm(Job job)
    {
        Amount -= restoreNeedAmount;
    }

    public void CompleteJobCrit(Job job)
    {
        Amount -= restoreNeedAmount / 4;
    }

    public Need Clone()
    {
        return new Need(this);
    }
}
