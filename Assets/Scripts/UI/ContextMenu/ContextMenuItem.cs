#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuItem : MonoBehaviour
{
    public ContextMenu ContextMenu;
    public Text text;
    public ContextMenuAction Action;
    private MouseController mouseController;

    public void Start()
    {
        mouseController = WorldController.Instance.mouseController;
    }

    /// <summary>
    /// Builds the interface.
    /// </summary>
    public void BuildInterface()
    {
        text.text = LocalizationTable.GetLocalization(Action.LocalizationKey);
    }

    /// <summary>
    /// Raises the click event.
    /// </summary>
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
