using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIRescaler : MonoBehaviour {
    public int lastScreenWidth = 0;

    void Start(){
        lastScreenWidth = Screen.width;
        StartCoroutine("AdjustScale");
        Debug.LogWarning(lastScreenWidth);
    }

    void Update(){
        if( lastScreenWidth != Screen.width ){
            lastScreenWidth = Screen.width;
            StartCoroutine("AdjustScale");
        }

    }

    IEnumerator AdjustScale(){
        Debug.LogWarning(lastScreenWidth);
        this.GetComponent<CanvasScaler>().scaleFactor = Screen.width / 1920f;
        yield return null;
    }
}