using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using System;
using ProjectPorcupine.Localization;

public class DateTimeUpdater : MonoBehaviour {
    private Text textComponent;
	// Use this for initialization
	void Start () 
    {
        textComponent = this.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        TimeManager tm = TimeManager.Instance;
        WorldTime time = tm.WorldTime;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(LocalizationTable.GetLocalization("time_string", time));
        sb.Append(LocalizationTable.GetLocalization("date_string", time));
        textComponent.text = sb.ToString();
	}
}
