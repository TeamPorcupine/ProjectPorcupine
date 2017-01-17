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
        int time = (int)(tm.GameTime * 90);
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

        int month = (days / 15) % 12;
        string monthString = ((Month)month).ToString();
        month += 1;
        int dayOfMonth = (days % 15) + 1;
        int year = 2999 + days / 180;
        sb.AppendFormat("{0}:{1:00} {2}\n", hours, minutes, pm ? "pm" : "am");
        sb.AppendFormat("{0} {1}, {2}", monthString, dayOfMonth, year);
        textComponent.text = sb.ToString();
	}

    private enum Month {January, February, March, April, May, June, July, August, September, October, November, December}
}
