#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorkerInfo : MonoBehaviour
{

    public Text textCharacter;

    public Button buttonBuild;
    Text textBuild;

    public Button buttonSleep;
    Text textSleep;

    public Button buttonCraft;
    Text textCraft;

    public Button buttonHaul;
    Text textHaul;

    int maxPriority = 4;

    void OnClickBuild()
    {
        CycleThroughNumbers(textBuild, "Build");
    }

    void OnClickSleep()
    {
        CycleThroughNumbers(textSleep, "Sleep");
    }

    void OnClickCraft()
    {
        CycleThroughNumbers(textCraft, "Craft");
    }

    void OnClickHaul()
    {
        CycleThroughNumbers(textHaul, "Haul");
    }

    void CycleThroughNumbers(Text buttonText, string priorityName)
    {
        int tempInt = Convert.ToInt32(buttonText.text);

        tempInt++;

        if (tempInt > maxPriority)
            tempInt = 1;
        
        buttonText.text = tempInt.ToString();
        CallUpdatePriority(priorityName, tempInt);
    }

    void CallUpdatePriority(string priorityNaame, int newValue)
    {
        Debug.Log("You changed " + textCharacter.text + "'s " + priorityNaame + " priority to " + newValue);
    }
        
    void Start()
    {
        buttonBuild.onClick.AddListener(delegate
            {
                OnClickBuild();
            });

        buttonSleep.onClick.AddListener(delegate
            {
                OnClickSleep();
            });

        buttonCraft.onClick.AddListener(delegate
            {
                OnClickCraft();
            });

        buttonHaul.onClick.AddListener(delegate
            {
                OnClickHaul();
            });

        textBuild = buttonBuild.GetComponentInChildren<Text>();
        textSleep = buttonSleep.GetComponentInChildren<Text>();
        textCraft = buttonCraft.GetComponentInChildren<Text>();
        textHaul = buttonHaul.GetComponentInChildren<Text>();
    }
}
