using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class CurrencyDisplay : MonoBehaviour {

    public Text text;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        string[] currencies = World.Current.Wallet.GetCurrencyNames();
        StringBuilder text = new StringBuilder();

        // Populate the text box
        foreach (string currency in currencies)
        {
            text.Append(currency + ":");
            text.Append(World.Current.Wallet[currency].Balance);
            text.AppendLine();
        }

        // Don't question it ok.
        this.text.text = text.ToString();
	}
}
