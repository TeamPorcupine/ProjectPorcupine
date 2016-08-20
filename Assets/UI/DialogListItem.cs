﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DialogListItem : MonoBehaviour, IPointerClickHandler
{
	public string fileName;
	public InputField inputField;
    public delegate void doubleClickAction();
    public doubleClickAction doubleclick;


    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
	{
		// Our job is to take our text label and 
		// copy it into a target field.


		inputField.text = fileName;
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
                doubleclick();
        }
     }

	#endregion


}
