using System;

public class ContextMenuAction
{
    public bool RequiereCharacterSelected;
    public string Text;
    
    public Action<ContextMenuAction, Character> Action;

    public void OnClick(MouseController mouseController)
    {
        if (Action != null)
        {
            if (RequiereCharacterSelected)
            {
                if (mouseController.mySelection != null)
                {
                    ISelectable actualSelection =
                        mouseController.mySelection.stuffInTile[mouseController.mySelection.subSelection];
                    if (actualSelection is Character)
                    {
                        Action(this, actualSelection as Character);
                    }
                }
            }
            else
            {
                Action(this, null);
            }
        }
    }
}