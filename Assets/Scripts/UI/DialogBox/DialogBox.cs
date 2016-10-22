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
    public static readonly Color ListPrimaryColor = new Color32(0, 149, 217, 80);
    public static readonly Color ListSecondaryColor = new Color32(0, 149, 217, 160);

    private bool openedWhileModal = false;

    public virtual void ShowDialog()
    {
        openedWhileModal = WorldController.Instance.IsModal ? true : false;

        WorldController.Instance.IsModal = true;

        gameObject.transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    public virtual void CloseDialog()
    {
        if (!openedWhileModal)
        {
            WorldController.Instance.IsModal = false;
        }

        gameObject.SetActive(false);
    }
}