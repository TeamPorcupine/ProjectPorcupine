#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This is holds the original and the selected colors for the colorButton.
/// </summary>
[System.Serializable]
public struct ButtonUpdateColorBlock
{
    public Color textOriginalColor;
    public Color textSelectedColor;

    public Color imageOriginalColor;
    public Color imageSelectedColor;
}

/// <summary>
/// Just a nice extension.
/// This will allow you to set colors that will happen automatically when clicked.
/// They will have to be unselected via a public method.
/// </summary>
[RequireComponent(typeof(Button))]
public class ColorButton : MonoBehaviour
{
    [SerializeField]
    private ButtonUpdateColorBlock colorSelectGroup = new ButtonUpdateColorBlock();

    private Text updateText;
    private Button button;

    public void SetText(string text)
    {
        if (updateText != null)
        {
            updateText.text = text;
        }
    }

    public void RevertColor()
    {
        button.image.color = colorSelectGroup.imageOriginalColor;

        if (updateText != null)
        {
            updateText.color = colorSelectGroup.textOriginalColor;
        }
    }

    public void SelectColor()
    {
        button.image.color = colorSelectGroup.imageSelectedColor;

        if (updateText != null)
        {
            updateText.color = colorSelectGroup.textSelectedColor;
        }
    }

    private void Awake()
    {
        updateText = gameObject.GetComponentInChildren<Text>();
        button = gameObject.GetComponent<Button>();

        button.onClick.AddListener(
            () =>
            {
                SettingsMenu.DisplayCategory(gameObject.name);
            });
    }
}