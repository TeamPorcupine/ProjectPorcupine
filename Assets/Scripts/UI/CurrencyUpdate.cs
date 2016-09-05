﻿
#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyUpdate : MonoBehaviour
{
    public Text text;
    
    private static List<Currency> currencyBalances;
    
    // Event triggered function which updates the currency status text
    public void UpdateCurrency(Currency currencyToUpdate)
    {
        StringBuilder builder = new StringBuilder();
        foreach (Currency currency in currencyBalances)
        {
            builder.AppendFormat("{0:N2} {1}", currency.Balance, currency.ShortName);
            builder.AppendLine();

        }

		Debug.Log (builder.ToString ());
		if (text != null)
        { 	
			text.text = builder.ToString ();
			
		}
		
	}
	
	
	// Adds UpdateCurrency to all the currencies
	public void AddWallet (Wallet wallet) {
		if (currencyBalances == null) {


            currencyBalances = new List<Currency>();
            
			
		}
		
		foreach (KeyValuePair<string, Currency> kvp in wallet.Currencies) {
			
			Debug.Log (kvp.Value.Name + ":" + kvp.Value.Balance);
			


			kvp.Value.BalanceChanged += UpdateCurrency;
			currencyBalances.Add (kvp.Value);
			UpdateCurrency (kvp.Value);
		
		}
		
	}


    private void Start()
    {
        AddWallet(World.Current.Wallet);
    }

}
