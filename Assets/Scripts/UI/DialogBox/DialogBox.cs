#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using UnityEngine;

public class DialogBox : MonoBehaviour
{
    public static readonly Color ListPrimaryColor = new Color32(0, 149, 217, 80);
    public static readonly Color ListSecondaryColor = new Color32(0, 149, 217, 160);

    private bool openedWhileModal = false;

    public DialogBoxResult Result { get; set; }

    public Action Closed { get; set; }

    public virtual void ShowDialog()
    {
        openedWhileModal = GameController.Instance.IsModal ? true : false;

        GameController.Instance.IsModal = true;

        GameController.Instance.soundController.OnButtonSFX();

        gameObject.transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    public virtual void CloseDialog()
    {
        InvokeClosed();
        
        Closed = null;        

        UnityDebugger.Debugger.Log("ModDialogBox", "openedWhileModal=" + openedWhileModal.ToString());
        if (!openedWhileModal)
        {
            GameController.Instance.IsModal = false;
        }

        GameController.Instance.soundController.OnButtonSFX();
        
        gameObject.SetActive(false);
    }

    public void SetClosedAction(string funcName)
    {
        Closed = () => FunctionsManager.Get("ModDialogBox").Call(funcName, Result); 
    }

    private void InvokeClosed()
    {
        if (Closed != null)
        {
            Closed();
            Closed = null;
        }
    }
}
