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

    public ButtonExtended buttonBuild;
    Text textBuild;

    public ButtonExtended buttonSleep;
    Text textSleep;

    public ButtonExtended buttonCraft;
    Text textCraft;

    public ButtonExtended buttonHaul;
    Text textHaul;

    int maxPriority = 4;
    int minPriority = 1;

    void OnLeftClickBuild()
    {
        CycleUpNumbers(textBuild, "Build");
    }

    void OnRightClickBuild()
    {
        CycleDownNumbers(textBuild, "Build");
    }

    void OnClickBuildSetMax()
    {
        SetMax(textBuild, "Build");
    }

    void OnClickBuildSetMin()
    {
        SetMin(textBuild, "Build");
    }

    void OnLeftClickSleep()
    {
        CycleUpNumbers(textSleep, "Sleep");
    }

    void OnRightClickSleep()
    {
        CycleDownNumbers(textSleep, "Sleep");
    }

    void OnLeftClickCraft()
    {
        CycleUpNumbers(textCraft, "Craft");
    }

    void OnRightClickCraft()
    {
        CycleDownNumbers(textCraft, "Craft");
    }

    void OnLeftClickHaul()
    {
        CycleUpNumbers(textHaul, "Haul");
    }

    void OnRightClickHaul()
    {
        CycleDownNumbers(textHaul, "Haul");
    }

    void CycleUpNumbers(Text buttonText, string priorityName)
    {
        int tempInt = Convert.ToInt32(buttonText.text);

        tempInt++;

        if (tempInt > maxPriority)
        {
            tempInt = 1;
        }
        
        buttonText.text = tempInt.ToString();
        CallUpdatePriority(priorityName, tempInt);
    }

    void CycleDownNumbers(Text buttonText, string priorityName)
    {
        int tempInt = Convert.ToInt32(buttonText.text);

        tempInt--;

        if (tempInt < minPriority)
        {
            tempInt = 4;
        }

        buttonText.text = tempInt.ToString();
        CallUpdatePriority(priorityName, tempInt);
    }

    void SetMax(Text buttonText, string priorityName)
    {
        buttonText.text = maxPriority.ToString();
        CallUpdatePriority(priorityName, maxPriority);
    }

    void SetMin(Text buttonText, string priorityName)
    {
        buttonText.text = minPriority.ToString();
        CallUpdatePriority(priorityName, minPriority);
    }

    void CallUpdatePriority(string priorityNaame, int newValue)
    {
        Debug.Log("You changed " + textCharacter.text + "'s " + priorityNaame + " priority to " + newValue);
    }
        
    void Start()
    {
        buttonBuild.leftClick.AddListener(delegate
            {
                OnLeftClickBuild();
            });

        buttonBuild.rightClick.AddListener(delegate
            {
                OnRightClickBuild();
            });

        buttonBuild.controlLeftClick.AddListener(delegate
            {
                OnClickBuildSetMax();
            });

        buttonBuild.controlRightClick.AddListener(delegate
            {
                OnClickBuildSetMin();
            });
        
        buttonSleep.leftClick.AddListener(delegate
            {
                OnLeftClickSleep();
            });

        buttonSleep.rightClick.AddListener(delegate
            {
                OnRightClickSleep();
            });

        buttonCraft.leftClick.AddListener(delegate
            {
                OnLeftClickCraft();
            });

        buttonCraft.rightClick.AddListener(delegate
            {
                OnRightClickCraft();
            });

        buttonHaul.leftClick.AddListener(delegate
            {
                OnLeftClickHaul();
            });

        buttonHaul.rightClick.AddListener(delegate
            {
                OnRightClickHaul();
            });

        textBuild = buttonBuild.GetComponentInChildren<Text>();
        textSleep = buttonSleep.GetComponentInChildren<Text>();
        textCraft = buttonCraft.GetComponentInChildren<Text>();
        textHaul = buttonHaul.GetComponentInChildren<Text>();
    }
}
