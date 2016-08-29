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

/// Every frame, this script checks to see which tile
/// is under the mouse and then updates the GetComponent<Text>.text
/// parameter of the object it is attached to.
public class MouseOverTileTypeText : MonoBehaviour
{
    private Text text;
    private MouseController mouseController;

    // Use this for initialization.
    private void Start()
    {
        text = GetComponent<Text>();

        if (text == null)
        {
            Debug.ULogErrorChannel("MouseOver", "No 'Text' UI component on this object.");
            this.enabled = false;
            return;
        }

        mouseController = WorldController.Instance.mouseController;
        if (mouseController == null)
        {
            Debug.ULogErrorChannel("MouseOver", "How do we not have an instance of mouse controller?");
            return;
        }
    }

    // Update is called once per frame.
    private void Update()
    {
        Tile t = mouseController.GetMouseOverTile();
        string tileType = "Unknown";

        if (t != null)
        {
            tileType = t.Type.ToString();
        }

        text.text = "Tile Type: " + tileType;
    }
}
