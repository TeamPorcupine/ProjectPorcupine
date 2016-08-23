using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Need {
    public string needType;
    public string localisationID;
    public string Name;
    protected float growthRate;
    float _amount = 0;
    public float Amount { get { return _amount; } set
        {
            float f = value;
            if (f < 0)
                f = 0;
            if (f > 100)
                f = 100;
            _amount = f;
        }
    }
    protected bool highToLow = true;
    public Character character;
    public Furniture restoreNeedFurn { get; protected set; }
    public float restoreNeedTime { get; protected set; }
    protected float restoreNeedAmount = 100;
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
    public bool completeOnFail { get; protected set; }
    protected float addedInVacuum;
    protected float dps;
    // Use this for initialization
    public Need ()
    {
        Amount = 0;
    }
    protected Need (Need other)
    {
        Amount = 0;
        this.needType = other.needType;
        this.localisationID = other.localisationID;
        this.Name = other.Name;
        this.growthRate = other.growthRate;
        this.highToLow = other.highToLow;
        this.restoreNeedFurn = other.restoreNeedFurn;
        this.restoreNeedTime = other.restoreNeedTime;
        this.restoreNeedAmount = other.restoreNeedAmount;
        this.completeOnFail = other.completeOnFail;
        this.addedInVacuum = other.addedInVacuum;
        this.dps = other.dps;
    }
    
    // Update is called once per frame
    public void Update (float deltaTime)
    {
        Amount += growthRate * deltaTime;
        if (character != null && character.CurrTile.Room != null && character.CurrTile.Room.GetGasPressure ("O2") < 0.15)
        {
            Amount += (addedInVacuum - (addedInVacuum * (character.CurrTile.Room.GetGasPressure ("O2") * 5))) * deltaTime;
        }
        if (Amount > 75 && character.myJob.isNeed == false)
        {
            Debug.Log (character.name + " needs " + Name);
            character.AbandonJob (false);
        }
        if (Amount == 100 && character.myJob.critical == false)
        {
            Debug.Log (character.name + " failed their " + Name + " need.");
            character.AbandonJob (false);
        }
        if (Amount == 100)
        {
            //FIXME: Insert need fail damage code here.
        }
    }

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        //Debug.Log("ReadXmlPrototype");

        needType = reader_parent.GetAttribute("needType");

        XmlReader reader = reader_parent.ReadSubtree();


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
                restoreNeedFurn = World.current.furniturePrototypes[reader.ReadContentAsString()];
                break;
            case "RestoreNeedTime":
                reader.Read();
                restoreNeedTime = reader.ReadContentAsFloat();
                break;
            case "Damage":
                reader.Read();
                dps = reader.ReadContentAsFloat();
                break;
            case "CompleteOnFail":
                reader.Read();
                completeOnFail = reader.ReadContentAsBoolean();
                break;
            case "HighToLow":
                reader.Read();
                highToLow = reader.ReadContentAsBoolean();
                break;
            case "GrowthRate":
                reader.Read ();
                growthRate = reader.ReadContentAsFloat();
                break;
            case "GrowthInVacuum":
                reader.Read ();
                addedInVacuum = reader.ReadContentAsFloat();
                break;
            case "RestoreNeedAmount":
                reader.Read ();
                restoreNeedAmount = reader.ReadContentAsFloat();
                break;
            case "Localization":
                reader.Read ();
                localisationID = reader.ReadContentAsString();
                break;
            }
        }
    }

    public void CompleteJobNorm (Job j)
    {
        Amount -= restoreNeedAmount;
    }
    public void CompleteJobCrit (Job j)
    {
        Amount -= restoreNeedAmount/4;
    }
    public Need Clone()
    {
        return new Need(this);
    }
}
