#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Can be used to create message boxes with info to the user (with an OK button to close or none)
/// or prompt dialogs with a choice of "Yes", "No" and/or "Cancel" buttons.
/// </summary>
[MoonSharpUserData]
public class DialogBoxPromptOrInfo : DialogBox
{
    private float standardWidth = 320f;

    public DialogBoxResult Buttons { get; set; }

    /// <summary>
    /// Define the buttons that should appear in the dialog (yes, no, cancel).
    /// </summary>
    public void SetButtons(params DialogBoxResult[] buttonsToSet)
    {
        Transform buttons = gameObject.transform.Find("Buttons");
        buttons.gameObject.SetActive(true);

        foreach (Transform button in buttons)
        {
            button.gameObject.SetActive(false);
        }

        if (Array.Exists(buttonsToSet, element => element == DialogBoxResult.Yes))
        {
            gameObject.transform.Find("Buttons/Button - Yes").gameObject.SetActive(true);
        }

        if (Array.Exists(buttonsToSet, element => element == DialogBoxResult.No))
        {
            gameObject.transform.Find("Buttons/Button - No").gameObject.SetActive(true);
        }

        if (Array.Exists(buttonsToSet, element => element == DialogBoxResult.Cancel))
        {
            gameObject.transform.Find("Buttons/Button - Cancel").gameObject.SetActive(true);
        }
    }

    public void SetButtons(Table buttons)
    {
        Transform buttonsGO = gameObject.transform.Find("Buttons");
        buttonsGO.gameObject.SetActive(true);

        foreach (Transform button in buttonsGO)
        {
            button.gameObject.SetActive(false);
        }

        UnityDebugger.Debugger.Log("ModDialogBox", "Table length:" + buttons.Length.ToString());
        for (int i = 1; i <= buttons.Length; i++)
        {
            switch (buttons.RawGet(i).ToObject<int>())
            {
                case 0:
                    gameObject.transform.Find("Buttons/Button - Yes").gameObject.SetActive(true);
                    break;
                case 1:
                    gameObject.transform.Find("Buttons/Button - No").gameObject.SetActive(true);
                    break;
                case 2:
                    gameObject.transform.Find("Buttons/Button - Cancel").gameObject.SetActive(true);
                    break;
                case 3:
                    gameObject.transform.Find("Buttons/Button - Okay").gameObject.SetActive(true);
                    break;
            }
        }
    }

    /// <summary>
    /// This creates a simple message box with an ok button.
    /// </summary>
    /// <param name="infoText">Text to show.</param>
    public void SetAsInfo(string infoText)
    {
        SetPrompt(infoText);

        Transform buttons = gameObject.transform.Find("Buttons");
        buttons.gameObject.SetActive(true);

        foreach (Transform button in buttons)
        {
            button.gameObject.SetActive(false);
        }

        gameObject.transform.Find("Buttons/Button - Okay").gameObject.SetActive(true);
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

        gameObject.transform.Find("Prompt").GetComponent<Text>().text = "dummy_message_text";

        gameObject.transform.Find("Buttons/Button - Yes").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons/Button - No").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons/Button - Cancel").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons/Button - Okay").gameObject.SetActive(false);
        gameObject.transform.Find("Buttons").gameObject.SetActive(false);

        base.CloseDialog();
    }

    /// <summary>
    /// Sets the text to show in the dialog.
    /// </summary>
    /// <param name="key">Loalization key for the prompt's text.</param>
    public void SetPrompt(string key, params string[] additionalValues)
    {
        string localized = ProjectPorcupine.Localization.LocalizationTable.GetLocalization(key, additionalValues);

        gameObject.transform.Find("Prompt").GetComponent<Text>().text = localized;
    }

    public void SetWidth(float width)
    {
        Vector2 size = gameObject.GetComponent<RectTransform>().sizeDelta;
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, size.y);
    }
}
