using UnityEngine;
using System.Collections;

public class Need {
	public string needType;
	public string Name;
	float growthRate;
	public float Amount;
	bool invertDisplay;
	public Character character;
	Furniture restoreNeedFurn;
	float restoreNeedTime;
	public string DisplayAmount
	{
		get
		{
			if (invertDisplay)
			{
				return (100 - (int)Amount) + "%";
			}
			return ((int)Amount) + "%";
		}
	}
	bool completeOnFail;
	float addedInVaccum;
	float dps;
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
			Amount += (addedInVaccum - (addedInVaccum * (character.CurrTile.room.GetGasPressure ("O2") / 0.2))) * deltaTime;
		}
		//TODO: What happens at 100%?
	}
}
