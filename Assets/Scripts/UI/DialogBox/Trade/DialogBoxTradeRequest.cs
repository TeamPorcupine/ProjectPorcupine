using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class DialogBoxTradeRequest : MonoBehaviour {

    public GameObject toggles;
    public Text itemName;

    public TraderPotentialInventory request;
    public RequestLevel requestLevel;

    public void UpdateRequestLevel (string level)
    {
        try
        {
            requestLevel = (RequestLevel)Enum.Parse(typeof(RequestLevel), level);
        }
        catch (Exception e)
        {
            Debug.LogError("Request toggle string invalid" + Environment.NewLine + e.StackTrace);
            
        }
    }

}
