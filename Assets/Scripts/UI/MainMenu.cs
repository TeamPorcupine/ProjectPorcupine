#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        RenderButtons();
    }

    private void RenderButtons()
    {
        UnityEngine.Object buttonPrefab = Resources.Load("UI/Components/MenuButton");

        GameObject newWorldButton = CreateButtonGO(buttonPrefab, "New World", "new_world");
        newWorldButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            if (!GameController.Instance.IsModal)
            {
                SceneController.Instance.ConfigureNewWorld();
            }
        });

        GameObject loadWorldButton = CreateButtonGO(buttonPrefab, "Load", "load");
        loadWorldButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            if (!GameController.Instance.IsModal)
            {
                MainMenuController.Instance.dialogBoxManager.dialogBoxLoadGame.ShowDialog();
            }
        });

        GameObject settingsButton = CreateButtonGO(buttonPrefab, "Settings", "menu_settings");
        settingsButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            if (!GameController.Instance.IsModal)
            {
                SettingsMenu.Open();
            }
        });

        GameObject quitButton = CreateButtonGO(buttonPrefab, "Quit", "menu_quit");
        quitButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            if (!GameController.Instance.IsModal)
            {
                SceneController.Instance.QuitGame();
            }
        });
    }

    private GameObject CreateButtonGO(UnityEngine.Object buttonPrefab, string name, string localizationCode)
    {
        GameObject buttonGameObject = (GameObject)Instantiate(buttonPrefab);
        buttonGameObject.transform.SetParent(this.transform, false);
        buttonGameObject.name = "Button " + name;
        TextLocalizer textLocalizer = buttonGameObject.transform.GetComponentInChildren<TextLocalizer>();

        textLocalizer.formatValues = new string[] { LocalizationTable.GetLocalization(localizationCode) };
        textLocalizer.defaultText = localizationCode;

        return buttonGameObject;
    }
}