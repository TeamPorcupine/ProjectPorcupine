using UnityEngine;
using UnityEngine.UI;

public class ContextMenuLayout : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		RecalcSize ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		RecalcSize ();
	}
		
	public void RecalcSize()
	{
		Vector2 size = this.GetComponent<RectTransform> ().sizeDelta;
		size.y = size.y * totalTextSize ();
		this.GetComponent<RectTransform>().sizeDelta = size;
		Debug.Log ("CML -- Are we running");
	}
	public int totalTextSize()
	{
		int textSize = 0;
		ContextMenuItem[] cMI = this.GetComponentsInChildren<ContextMenuItem> ();
		for (int i = 0; i < cMI.Length; i++) {
			textSize += cMI [i].text.text.Length;
		}
		return textSize;
	}
}
