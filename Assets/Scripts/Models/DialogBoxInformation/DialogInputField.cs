using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogInputField : DialogControl {

	public void UpdateText () {
        result = GetComponent<InputField>().text;
	}
}
