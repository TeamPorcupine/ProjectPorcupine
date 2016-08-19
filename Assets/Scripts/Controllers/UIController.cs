﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour 
{


	// Use this for initialization
	void Start () 
    {
//        int a = Button.Length;
//        for (int i = 0; i < a; i++)
//        {
//            Instantiate(Button[i], ParentObj.transform);
//        }
        GameObject topMenu = GameObject.Find("TopMenu");
        GameObject newWorld = Instantiate(Resources.Load("Button - New World"), topMenu.transform) as GameObject;
        GameObject load = Instantiate(Resources.Load("Button - Load"), topMenu.transform) as GameObject;
        GameObject save = Instantiate(Resources.Load("Button - Save"), topMenu.transform) as GameObject;
        GameObject pathTest = Instantiate(Resources.Load("Button - Pathfinding Test"), topMenu.transform) as GameObject;
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}
}
