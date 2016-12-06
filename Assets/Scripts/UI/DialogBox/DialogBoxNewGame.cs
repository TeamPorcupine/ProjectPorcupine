#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxNewGame : DialogBox
{
    public InputField Height;
    public InputField Width;
    public InputField Depth;
    public Toggle GenerateAsteroids;

    public override void ShowDialog()
    {
        base.ShowDialog();
    }

    public void OkayWasClicked()
    {
        int height = int.Parse(Height.text);
        int width = int.Parse(Width.text);
        int depth = int.Parse(Depth.text);
        SceneController.Instance.LoadNewWorld(height, width, depth, GenerateAsteroids.isOn);
    }
}
