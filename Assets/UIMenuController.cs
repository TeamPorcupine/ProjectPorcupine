﻿using UnityEngine;
using System.Collections;

public class UIMenuController : MonoBehaviour {

    // This is the parent of the menus
    Transform parent;

	// Use this for initialization
	void Awake () 
    {
        parent = this.gameObject.transform;

        AddMenu("MenuTop");
        AddMenu("MenuSubHolder");
        AddMenu("MenuBottom");
        AddMenu("MenuConstruction");
	}

    void Start()
    {
        AddMenu("MenuRight");
    }

	// Update is called once per frame
	void Update () 
    {

	}

    void AddMenu(string menuName)
    {
        GameObject tempGoObj;

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/" + menuName));
        tempGoObj.name = menuName;

        tempGoObj.transform.SetParent(parent,false);

        //Transform childTransform = child.transform;
        //childTransform.parent = parent;

    }
}
