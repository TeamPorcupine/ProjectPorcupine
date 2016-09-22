﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Can be used to create message boxes with info to the user (with an OK button to close or none)
/// or prompt dialogs with a choice of "Yes", "No" and/or "Cancel" buttons.
/// </summary>
public class DialogBoxPromptOrInfo : DialogBox
{
    public DialogBoxResult Result { get; set; }

    public DialogBoxResult Buttons { get; set; }

    private float standardWidth = 320f;

    public Action Closed { get; set; }

    /// <summary>
    /// Define the buttons that should appear in the dialog (yes, no, cancel).
    /// </summary>
    /// <param name="buttonsToSet">An combination enum built with bitwise ORs to define the buttons.</param>
    public void SetButtons(DialogBoxResult buttonsToSet)
    {
        gameObject.transform.Find("Buttons").gameObject.SetActive(true);

        if ((buttonsToSet & DialogBoxResult.Yes) == DialogBoxResult.Yes)
        {
            gameObject.transform.Find("Buttons/Button - Yes").gameObject.SetActive(true);
        }

        if ((buttonsToSet & DialogBoxResult.No) == DialogBoxResult.No)
        {
            gameObject.transform.Find("Buttons/Button - No").gameObject.SetActive(true);
        }

        if ((buttonsToSet & DialogBoxResult.Cancel) == DialogBoxResult.Cancel)
        {
            gameObject.transform.Find("Buttons/Button - Cancel").gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// This creates a simple message box with an ok button.
    /// </summary>
    /// <param name="infoText">Text to show.</param>
    public void SetAsInfo(string infoText)
    {
        gameObject.transform.Find("Buttons").gameObject.SetActive(true);

        SetPrompt(infoText);
        gameObject.transform.Find("Buttons/Button - Ok").gameObject.SetActive(true);
    }

    public void YesButtonClick()
    {
        Result = DialogBoxResult.Yes;
        CloseDialog();
    }

    public void NoButtonClick()
    {
        Result = DialogBoxResult.No;
        CloseDialog();
    }

    public void CancelButtonClick()
    {
        Result = DialogBoxResult.Cancel;
        CloseDialog();
    }

    public void OkButtonClick()
    {
        CloseDialog();
    }

    public override void CloseDialog()
    {
        SetWidth(standardWidth);

        gameObject.transform.Find("Prompt").GetComponent<Text>().text = "Set prompt text with SetPrompt() !!!";

        gameObject.transform.Find("Buttons/Button - Yes").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons/Button - No").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons/Button - Cancel").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons/Button - Ok").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons").gameObject.SetActive(false);

        base.CloseDialog();

        InvokeClosed();

        Closed = null;
    }

    /// <summary>
    /// Sets the text to show in the dialog.
    /// </summary>
    /// <param name="prompt"></param>
    public void SetPrompt(string prompt)
    {
        gameObject.transform.Find("Prompt").GetComponent<Text>().text = prompt;
    }

    public void SetWidth(float width)
    {
        Vector2 size = gameObject.GetComponent<RectTransform>().sizeDelta;
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, size.y);
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