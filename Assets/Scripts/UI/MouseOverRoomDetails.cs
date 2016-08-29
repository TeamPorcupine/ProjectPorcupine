#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// Every frame, this script checks to see which tile
/// is under the mouse and then updates the GetComponent<Text>.text
/// parameter of the object it is attached to.
public class MouseOverRoomDetails : MonoBehaviour
{
    private Text text;
    private MouseController mouseController;

    // Use this for initialization.
    private void Start()
    {
        text = GetComponent<Text>();

        if (text == null)
        {
            Debug.ULogErrorChannel("MouseOver", "MouseOverTileTypeText: No 'Text' UI component on this object.");
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

        if (t == null || t.Room == null)
        {
            text.text = string.Empty;
            return;
        }

        string s = string.Empty;

        foreach (string gasName in t.Room.GetGasNames())
        {
            s += string.Format("{0}: ({1}) {2:0.000} atm ({3:0.0}%)\n", gasName, t.Room.ChangedGases(gasName), t.Room.GetGasPressure(gasName), t.Room.GetGasFraction(gasName) * 100);
        }

        text.text = s;
    }
}
