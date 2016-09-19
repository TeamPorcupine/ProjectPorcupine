#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using UnityEngine.UI;

public class DialogBoxPromptOrInfo : DialogBox
{
    public DialogBoxResult Result { get; set; }

    public DialogBoxResult Buttons { get; set; }

    public Action Closed { get; set; }

    public void SetButtons(DialogBoxResult buttonsToSet)
    {
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

    public void SetAsInfo(string infoText)
    {
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
        gameObject.transform.Find("Prompt").GetComponent<Text>().text = "Set prompt text with SetPrompt() !!!";

        gameObject.transform.Find("Buttons/Button - Yes").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons/Button - No").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons/Button - Cancel").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons/Button - Ok").gameObject.SetActive(false);

        base.CloseDialog();

        InvokeClosed();

        Closed = () => { };
    }

    public void SetPrompt(string prompt)
    {
        gameObject.transform.Find("Prompt").GetComponent<Text>().text = prompt;
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