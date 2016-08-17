using UnityEngine;

public class EscMenu : MonoBehaviour {

    public Canvas canvas;

	void Start () {
        canvas = GetComponent<Canvas>();
    }
	
	void Update () {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            ToggleMenu();
        }
	}

    public void ToggleMenu() {
        canvas.enabled = !canvas.enabled;
        WorldController.Instance.IsModal = canvas.enabled;
    }
}
