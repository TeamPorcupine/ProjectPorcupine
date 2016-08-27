#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;

public class ContextMenuAction
{    
    public Action<ContextMenuAction, Character> Action;

    public bool RequireCharacterSelected { get; set; }

    public string Text { get; set; }

    public void OnClick(MouseController mouseController)
    {
        if (Action != null)
        {
            if (RequireCharacterSelected)
            {
                if (mouseController.IsCharacterSelected())
                {
                    ISelectable actualSelection = mouseController.mySelection.GetSelectedStuff();
                    Action(this, actualSelection as Character);
                }
            }
            else
            {
                Action(this, null);
            }
        }
    }
}