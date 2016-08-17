using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DialogListItem : MonoBehaviour, IPointerClickHandler
{
	
	GameObject go;
    public InputField inputField;

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        // Our job is to take our text label and 
        // copy it into a target field.

        inputField.text = transform.GetComponentInChildren<Text>().text;
		go = GameObject.FindGameObjectWithTag("DeleteButton");
		go.GetComponent<Image>().color= new Color(255,255,255,255);
		go.transform.position = new Vector3(transform.GetComponentInChildren<Text>().transform.position.x + 110f, transform.GetComponentInChildren<Text>().transform.position.y - 8f);

    }

    #endregion
}
