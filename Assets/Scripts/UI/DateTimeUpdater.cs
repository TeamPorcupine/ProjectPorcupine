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
        TimeManager tm = TimeManager.Instance;
        WorldTime time = tm.WorldTime;
        textComponent.text = time.ToString();
	}
}
