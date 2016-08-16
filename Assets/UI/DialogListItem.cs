using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DialogListItem : MonoBehaviour, IPointerClickHandler {

	public InputField inputField;

	#region IPointerClickHandler implementation
	public void OnPointerClick(PointerEventData eventData) {
		// Our job is to take our text label and 
		// copy it into a target field.

		inputField.text = transform.GetComponentInChildren<Text>().text;
	}
	#endregion
}
