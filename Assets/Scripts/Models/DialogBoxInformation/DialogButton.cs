#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogButton : MonoBehaviour
{
    public void OnClicked()
    {
        string buttonName;
        buttonName = GetComponentInChildren<Text>().text;

        // These names should actually be something like "button_my" in order to be localized.
        // However, just to make sure, we replace this to avoid any problems.
        buttonName = buttonName.Replace(" ", "_");
        EventActions dialogEvents = transform.GetComponentInParent<DialogBoxLua>().events;

        if (dialogEvents.HasEvent("On" + buttonName + "Clicked") == true)
        {
            dialogEvents.Trigger("On" + buttonName + "Clicked", this);
        }
    }
}
