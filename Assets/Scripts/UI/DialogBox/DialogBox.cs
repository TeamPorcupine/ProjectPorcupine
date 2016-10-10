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

public class DialogBox : MonoBehaviour
{
    public DialogBoxResult Result { get; set; }

    public Action Closed { get; set; }

    private bool openedWhileModal = false;

    public virtual void ShowDialog()
    {
        openedWhileModal = WorldController.Instance.IsModal ? true : false;
        gameObject.transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    public virtual void CloseDialog()
    {
        if (!openedWhileModal)
        {
            WorldController.Instance.IsModal = false;
        }

        InvokeClosed();

        Closed = null;

        gameObject.SetActive(false);
    }

    public void setClosedAction(string funcName)
    {
        Closed = () => FunctionsManager.Get("ModDialogBox").Call(funcName, Result); 
    }

    private void InvokeClosed()
    {
        Action closed = Closed;
        if (closed != null)
        {
            closed();
            Closed = null;
        }
    }
}