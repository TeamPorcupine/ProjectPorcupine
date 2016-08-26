using System;

public class ContextMenuAction
{
    public bool RequireCharacterSelected { get; set; }
    public string Text { get; set; }
    
    public Action<ContextMenuAction, Character> Action;

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