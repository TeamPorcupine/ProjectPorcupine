using UnityEngine;
using System.Collections;

public class OpenKeyConsole : MonoBehaviour {

	// Use this for initialization

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.F12)) {
            this.gameObject.transform.GetChild(0).GetComponent<DevConsole>().ShowDialog();
        }	
	}
}
