using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class CurrencyUpdate : MonoBehaviour {

	public Text text;
	
	static Dictionary <Currency, string> currencyBalances;
	
	void Start () {
		
		AddWallet (World.Current.Wallet);
		
	}
	

	// Event triggered function which updates the currency status text
	void UpdateCurrency (Currency currencyToUpdate) {
		
		StringBuilder builder = new StringBuilder ();
		currencyBalances[currencyToUpdate] = currencyToUpdate.Balance.ToString ();
		foreach (KeyValuePair <Currency, string> kvp in currencyBalances) {
			
			builder.AppendFormat ("{0} : {1}", kvp.Key.Name, kvp.Value);
			builder.AppendLine ();
			
		}
		Debug.Log (builder.ToString ());
		if (text != null) {
			
			text.text = builder.ToString ();
			
		}
		
	}
	
	
	// Adds UpdateCurrency to all the currencies
	public void AddWallet (Wallet wallet) {
		if (currencyBalances == null) {
			currencyBalances = new Dictionary<Currency, string> ();
		}
		
		foreach (KeyValuePair<string, Currency> kvp in wallet.Currencies) {
			
			Debug.Log (kvp.Value.Name + ":" + kvp.Value.Balance);
			
			kvp.Value.balanceChanged += UpdateCurrency;
			currencyBalances.Add (kvp.Value, kvp.Value.Balance.ToString ());
			UpdateCurrency (kvp.Value);
		
		}
		
	}
	
	
}
