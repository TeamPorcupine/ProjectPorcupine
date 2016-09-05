using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/// <summary>
/// Saves trader data for trader persistence (traders return time and time again to rip you off)
/// </summary>
public class TradersController
{
	// TODO save trader data
	public Dictionary<string, Trader> traders;
		
	public void AddTrader (Trader t) {
			
		if (traders == null) {
				
			traders = new Dictionary<string, Trader>();
				
		}
			
		traders.Add (t.Name, t);
			
	}
		
	// Gets a random trader from the list
	public Trader RequestRandomTrader () {
			
		Trader joe = traders.Values.ToList()[UnityEngine.Random.Range (0, traders.Count)];
		joe.RefreshInventory ();
		return joe;
			
	}
		
		
		
		
		
}
