using UnityEngine;
using System.Collections;

public class Need {
	public string needType;
	public string Name;
	float growthRate;
	public float Amount;
	bool invertDisplay;
	public string DisplayAmount
	{
		get
		{
			if (invertDisplay)
			{
				return 100 - amount + "%";
			}
			return amount + "%";
		}
	}
	bool completeOnFail;
	float addedInVaccum;
	float dps;
	// Use this for initialization
	public Need () {
	
	}
	
	// Update is called once per frame
	void Update (float deltaTime) {
	
	}
}
