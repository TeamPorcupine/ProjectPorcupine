using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AutomaticHorizontalSize : MonoBehaviour {

    public float childWidth = 35f;

    // Use this for initialization
    void Start() {
        AdjustSize();
    }

    void Update() {
        AdjustSize();
    }

    public void AdjustSize() {
        Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
        size.x = this.transform.childCount * childWidth;
        this.GetComponent<RectTransform>().sizeDelta = size;
    }
}
