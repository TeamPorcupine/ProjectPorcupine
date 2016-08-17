using UnityEngine;

public class EscMenu : MonoBehaviour {

    public Canvas canvas;
    public DialogBoxLoadSaveGame loadGame;
    public DialogBoxLoadSaveGame saveGame;

    void Start () {
        canvas = GetComponent<Canvas>();
    }
	
	void Update () {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            ToggleMenu();
        }
	}

    public void ShowSaveDialog() {
        saveGame.ShowDialog();
    }

    public void ShowLoadDialog() {
        loadGame.ShowDialog();
    }

    public void ToggleMenu() {
        canvas.enabled = !canvas.enabled;
        WorldController.Instance.IsModal = canvas.enabled;
        if(!canvas.enabled) {
            loadGame.CloseDialog();
            saveGame.CloseDialog();
        }
    }
}
