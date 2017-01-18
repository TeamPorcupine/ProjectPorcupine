using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using System;

public class DateTimeUpdater : MonoBehaviour {
    private Text textComponent;
	// Use this for initialization
	void Start () 
    {
        textComponent = this.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        StringBuilder sb = new StringBuilder();
        TimeManager tm = TimeManager.Instance;
        int time = (int)(tm.WorldTime);
//        time *= 60;
        int seconds = time % 60;
        int minutes = (time / 60)  % 60;
        int hours = (time / 3600)  % 24;
        bool pm = (hours >= 12);
        if (pm)
        {
            hours -= 12;
        }

        if (hours == 0)
        {
            hours = 12;
        }

        int days = time / 86400;

        int quarter = ((days / 15) % 4) + 1;
        int dayOfQuarter = (days % 15) + 1;
        int year = 2999 + days / 60;
        sb.AppendFormat("{0}:{1:00} {2}\n", hours, minutes, pm ? "pm" : "am");
        sb.AppendFormat("Q{0} Day {1}, {2}", quarter, dayOfQuarter, year);
        textComponent.text = sb.ToString();
	}
}
