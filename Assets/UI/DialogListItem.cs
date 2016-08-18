using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DialogListItem : MonoBehaviour, IPointerClickHandler
{
    public string fileName;
    public InputField inputField;
    public GameObject OkayButton; 

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        // Our job is to take our text label and 
        // copy it into a target field.

        inputField.text = fileName;

        if (eventData.clickCount > 1)
        {
            OkayButton = GameObject.FindGameObjectWithTag("OkayButton");
            Button ok = OkayButton.GetComponent<Button>();
            ok.onClick.Invoke();
        }
    }
    

    #endregion
}
