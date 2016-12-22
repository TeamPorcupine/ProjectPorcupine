#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Every frame, this script checks to see which tile
/// is under the mouse and then updates the GetComponent<Text>.text
/// parameter of the object it is attached to.
/// </summary>
public abstract class MouseOver : MonoBehaviour 
{
    private Text text;
    private MouseController mouseController;

    // Update is called once per frame
    public void Update() 
    {
        Tile tile = mouseController.GetMouseOverTile();

        string infoString = "null";

        if (tile != null)
        {
            infoString = GetMouseOverString(tile);
        }

        text.text = infoString;
    }

    /// <summary>
    /// Obtains a string that represents info about the tile.
    /// </summary>
    /// <param name="tile">The in game tile that our mouse is currently over.</param>
    protected abstract string GetMouseOverString(Tile tile);

    // Use this for initialization.
    private void Start()
    {
        text = GetComponent<Text>();

        if (text == null)
        {
            UnityDebugger.Debugger.LogError("MouseOver", "MouseOver: No 'Text' UI component on this object.");
            this.enabled = false;
            return;
        }

        mouseController = WorldController.Instance.mouseController;
        if (mouseController == null)
        {
            UnityDebugger.Debugger.LogError("MouseOver", "How do we not have an instance of mouse controller?");
            this.enabled = false;
            return;
        }
    }
}
