using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour 
{
    public GameObject[] Button;
    public GameObject ParentObj;

	// Use this for initialization
	void Start () 
    {
        int a = Button.Length;
        for (int i = 0; i < a; i++)
        {
            Instantiate(Button[i], ParentObj.transform);
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}
}
