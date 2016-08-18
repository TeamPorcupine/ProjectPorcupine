using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DialogListItem : MonoBehaviour, IPointerClickHandler
{
    public string fileName;
    public InputField inputField;

    GameObject go;

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        // Our job is to take our text label and 
        // copy it into a target field.

        inputField.text = fileName;
        go = GameObject.FindGameObjectWithTag("DeleteButton");
        go.GetComponent<Image>().color= new Color(255,255,255,255);
        go.transform.position = new Vector3(transform.GetComponentInChildren<Text>().transform.position.x + 110f, transform.GetComponentInChildren<Text>().transform.position.y - 8f);
    }

    #endregion
}
