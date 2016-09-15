#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxTradeRequest : MonoBehaviour
{ 
    public GameObject toggles;
    public Text itemName;

    public TraderPotentialInventory request;
    public RequestLevel requestLevel;

    public void UpdateRequestLevel(string level)
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
