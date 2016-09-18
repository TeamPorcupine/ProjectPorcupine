using UnityEngine;
using UnityEngine.UI;

public class ContextMenuItemLayout : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		RecalcSize ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Debug.Log ("Are we running");
		RecalcSize ();
	}
		
	public void RecalcSize()
	{
		Vector2 size = this.GetComponent<RectTransform> ().sizeDelta;
		size.y = size.y * this.GetComponent<ContextMenuItem> ().text.text.Length;
		this.GetComponent<RectTransform>().sizeDelta = size;
		Debug.Log ("sizeDeltaXY:" + size);
	}
}
