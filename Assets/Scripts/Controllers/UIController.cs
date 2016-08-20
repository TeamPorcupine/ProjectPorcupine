using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	public WorldController wCont;
	public KeyCode escMenu;
	public GameObject escMenuCanvas;
	public GameObject topMenu;
	// Use this for initialization
	void Start () 
	{
		escMenuCanvas.SetActive(false);
		if (topMenu.transform.childCount != 0)
		{
			GameObject[] menuObjs = new GameObject[topMenu.transform.childCount ];
			for (int i = 0; i < topMenu.transform.childCount; i++)
			{
				menuObjs[i] = topMenu.transform.GetChild(i).gameObject;
			}
			AddToEscMenuAsArray(menuObjs);
			Destroy(topMenu);
		}

	}
	
	// Update is called once per frame
	void Update () 
	{
		//Im going to use isModal for now although i could do it entirely withen this 
		//class 
        if (Input.GetKeyDown(escMenu) && !wCont.IsModal )
		{
			wCont.IsModal = true;
			escMenuCanvas.SetActive(true);
		}
		else if (Input.GetKeyDown(escMenu) && wCont.IsModal)
		{
			wCont.IsModal = false;
			escMenuCanvas.SetActive(false);

		}
	}
	//Use to add anything to the escape menu needs the object a width and a height
	public void AddToEscMenu(GameObject obj,int x = 150, int y = 35)
	{
		LayoutElement layElm = obj.AddComponent<LayoutElement>();
		layElm.preferredHeight = y;
		layElm.preferredWidth = x;
		obj.transform.SetParent(escMenuCanvas.transform);
	}
	//Use for arrays of menu objs using default widths and height
	public void AddToEscMenuAsArray(GameObject[] obj)
	{
		for (int i = 0; i < obj.Length; i++)
		{
			AddToEscMenu(obj[i]);
		}
	
	}
	//Use for arrays of menu objs that dont use default width and height
	public void AddToEscMenuAsArray(GameObject[] obj, int[] x, int[] y)
	{
		for (int i = 0; i < obj.Length; i++)
		{
			AddToEscMenu(obj[i], x[i], y[i]);
		}


	}
}
