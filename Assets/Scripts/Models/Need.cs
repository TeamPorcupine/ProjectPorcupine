using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Need {
	public string needType;
	public string Name;
	protected float growthRate;
	public float Amount = 0;
	protected bool highToLow = true;
	public Character character;
	protected Furniture restoreNeedFurn;
	protected float restoreNeedTime;
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
	protected bool completeOnFail;
	protected float addedInVacuum;
	protected float dps;
	// Use this for initialization
	public Need ()
	{
	
	}
	
	// Update is called once per frame
	public void Update (float deltaTime)
	{
		Amount += growthRate * deltaTime;
		if (character != null && character.CurrTile.room.GetGasPressure ("O2") < 0.15)
		{
			Amount += (addedInVacuum - (addedInVacuum * (character.CurrTile.room.GetGasPressure ("O2") * 5))) * deltaTime;
		}
		//TODO: What happens at 100%?
		if (Amount > 75 && character.myJob.isNeed == false)
		{
			character.AbandonJob ();
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
			}
		}


	}
}
