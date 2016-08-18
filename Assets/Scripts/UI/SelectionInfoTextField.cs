﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ProjectPorcupine.Localization;

public class SelectionInfoTextField : MonoBehaviour
{

    public CanvasGroup canvasGroup;

    MouseController mc;
    Text txt;

    // Use this for initialization
    void Start()
    {
        mc = FindObjectOfType<MouseController>();
        txt = GetComponent<Text>();
    }
	
    // Update is called once per frame
    void Update()
    {
        if (mc.mySelection == null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return;
        }

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        ISelectable actualSelection = mc.mySelection.stuffInTile[mc.mySelection.subSelection];

        txt.text = LocalizationTable.GetLocalization(actualSelection.GetName()) + "\n" + LocalizationTable.GetLocalization(actualSelection.GetDescription()) + "\n" + actualSelection.GetHitPointString(); //TODO: Change the hitpoint stuff.
    }
}
