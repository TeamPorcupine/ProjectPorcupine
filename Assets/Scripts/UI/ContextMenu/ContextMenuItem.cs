using UnityEngine;
using UnityEngine.UI;

public class ContextMenuItem : MonoBehaviour
{
    private MouseController mouseController;
    public ContextMenu ContextMenu;
    public Text text;
    public ContextMenuAction Action;

    public void Start()
    {
        mouseController = WorldController.Instance.mouseController;
    }

    public void BuildInterface()
    {
        text.text = Action.Text;
    }

    public void OnClick()
    {
        if (Action != null)
        {
            Action.OnClick(mouseController);
        }

        if (ContextMenu != null)
        {
            ContextMenu.Close();
        }
    }
}