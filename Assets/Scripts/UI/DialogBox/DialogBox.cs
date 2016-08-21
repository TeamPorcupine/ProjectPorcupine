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

public class DialogBox : MonoBehaviour
{

    virtual public void ShowDialog()
    {
        WorldController.Instance.IsModal = true;
        gameObject.SetActive(true);
    }

    virtual public void CloseDialog()
    {
        WorldController.Instance.IsModal = false;
        gameObject.SetActive(false);
    }
        
}
