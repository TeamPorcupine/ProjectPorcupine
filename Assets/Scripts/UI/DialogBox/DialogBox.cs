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

public class DialogBox : MonoBehaviour
{
    public virtual void ShowDialog()
    {
        WorldController.Instance.IsModal = true;
        gameObject.SetActive(true);
    }

    public virtual void CloseDialog()
    {
        WorldController.Instance.IsModal = false;
        gameObject.SetActive(false);
    }

    public virtual void ToggleDialog()
    {
        if (this.isActiveAndEnabled)
        {
            this.CloseDialog();
        }
        else
        {
            this.ShowDialog();
        }
    }
}