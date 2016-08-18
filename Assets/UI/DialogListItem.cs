using UnityEngine;
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

        if (eventData.clickCount > 1)
        {
            if (doubleclick != null)
                doubleclick();
        }
    }

    #endregion

}
