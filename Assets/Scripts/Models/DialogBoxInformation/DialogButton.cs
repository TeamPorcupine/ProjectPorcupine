using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogButton : MonoBehaviour {

	public void OnClicked()
    {
        string buttonName;
        buttonName = GetComponentInChildren<Text>().text;
        buttonName = buttonName.Replace(" ", "_");
        EventActions dialogEvents = transform.GetComponentInParent<DialogBoxLua>().events;

        if (dialogEvents.HasEvent("On" + buttonName + "Clicked") == true)
        {
            dialogEvents.Trigger("On" + buttonName + "Clicked", this);
        }
    }
}
