using UnityEngine;
using ProjectPorcupine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private DialogBoxManager dialogManager;

    public void Start()
    {
        RenderButtons();
    }

    public void OnButtonNewWorld()
    {
       // dialogManager.dialogBoxPromptOrInfo.SetPrompt("message_creating_new_world");
      //  dialogManager.dialogBoxPromptOrInfo.ShowDialog();
        SceneManager.LoadScene("_SCENE_");
    }

    public void OnButtonLoadGame()
    {
        // dialogManager.dialogBoxLoadGame.ShowDialog();
    }


    // Quit the app whether in editor or a build version.
    public void OnButtonQuitGame()
    {
        // Maybe ask the user if he want to save or is sure they want to quit??
#if UNITY_EDITOR
        // Allows you to quit in the editor.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RenderButtons()
    {
        UnityEngine.Object buttonPrefab = Resources.Load("UI/Components/MenuButton");

        GameObject newWorldButton = CreateButtonGO(buttonPrefab, "New World", "new_world");
        newWorldButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnButtonNewWorld();
        });

        GameObject loadButton = CreateButtonGO(buttonPrefab, "Load", "load");
        loadButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnButtonLoadGame();
        });

        GameObject quitButton = CreateButtonGO(buttonPrefab, "Quit", "menu_quit");
        quitButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnButtonQuitGame();
        });
    }

    private GameObject CreateButtonGO(UnityEngine.Object buttonPrefab, string name, string localizationCode)
    {
        GameObject buttonGameObject = (GameObject)Instantiate(buttonPrefab);
        buttonGameObject.transform.SetParent(this.transform, false);
        buttonGameObject.name = "Button " + name;

        string localLocalizationCode = localizationCode;
        buttonGameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(localLocalizationCode) };

        return buttonGameObject;
    }
}
