#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

public class DialogButton : DialogControl
{
    public string buttonName;

    public void OnClicked()
    { 
        // These names should actually be something like "button_my" in order to be localized.
        // However, just to make sure, we replace this to avoid any problems.
        buttonName = buttonName.Replace(" ", "_");
        EventActions dialogEvents = transform.GetComponentInParent<ModDialogBox>().events;

        UnityDebugger.Debugger.Log("ModDialogBox", "Calling On" + buttonName + "Clicked function");

        if (dialogEvents.HasEvent("On" + buttonName + "Clicked") == true)
        {
            UnityDebugger.Debugger.Log("ModDialogBox", "Found On" + buttonName + "Clicked event");
            dialogEvents.Trigger<ModDialogBox>("On" + buttonName + "Clicked", transform.GetComponentInParent<ModDialogBox>());
        }
    }
}
