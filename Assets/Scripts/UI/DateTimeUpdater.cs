#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;
using UnityEngine.UI;

public class DateTimeUpdater : MonoBehaviour 
{
    private Text textComponent;

    public void Start()
    {
        textComponent = this.GetComponent<Text>();
    }

    public void Update() 
    {
        TimeManager tm = TimeManager.Instance;
        WorldTime time = tm.WorldTime;
        textComponent.text = time.ToString();
    }
}
