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

public class DialogListItem : MonoBehaviour, IPointerClickHandler
{
    public string fileName;
    public InputField inputField;
    public DoubleClickAction doubleclick;
    public Color currentColor;

    public delegate void DoubleClickAction();

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        // Our job is to take our text label and 
        // copy it into a target field.
        inputField.text = fileName;

        DialogListItem[] listItems = transform.parent.GetComponentsInChildren<DialogListItem>();
        foreach (DialogListItem listItem in listItems)
        {
            listItem.GetComponent<Image>().color = listItem.currentColor;
        }

        GetComponent<Image>().color = new Color32(0, 68, 101, 153);

        GameObject go = GameObject.FindGameObjectWithTag("DeleteButton");
        if (go != null)
        {
            go.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            Component text = transform.GetComponentInChildren<Text>();
            GetComponentInParent<DialogBoxLoadGame>().pressedDelete = true;
            GetComponentInParent<DialogBoxLoadGame>().SetFileItem(text);
        }

        if (eventData.clickCount > 1)
        {
            if (doubleclick != null)
            {
                doubleclick();
            }
        }
    }
    #endregion
}
