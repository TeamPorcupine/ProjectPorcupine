#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MouseOverTileTypeText : MonoBehaviour
{

    // Every frame, this script checks to see which tile
    // is under the mouse and then updates the GetComponent<Text>.text
    // parameter of the object it is attached to.

    Text myText;
    MouseController mouseController;

    // Use this for initialization
    void Start()
    {
        myText = GetComponent<Text>();

        if (myText == null)
        {
            Debug.LogError("MouseOverTileTypeText: No 'Text' UI component on this object.");
            this.enabled = false;
            return;
        }

        mouseController = WorldController.Instance.mouseController;
        if (mouseController == null)
        {
            Debug.LogError("How do we not have an instance of mouse controller?");
            return;
        }
    }
	
    // Update is called once per frame
    void Update()
    {
        Tile t = mouseController.GetMouseOverTile();
        string tileType = "Unknown";

        if (t != null)
        {
            tileType = t.Type.ToString();
        }

        myText.text = "Tile Type: " + tileType;
    }
}
