#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxQuitMenu : DialogBox
{
    private void Start()
    {
        UnityEngine.Object buttonPrefab = Resources.Load("UI/Components/MenuButton");

        GameObject quitButton = CreateButtonGO(buttonPrefab, "QuitGame", "menu_quit_game");
        quitButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            SceneController.Instance.QuitGame();
        });

        GameObject mainMenuButton = CreateButtonGO(buttonPrefab, "Quit To Main Menu", "menu_quit_to_menu");
        mainMenuButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            this.CloseDialog();
            SceneController.Instance.LoadMainMenu();
        });

        GameObject cancelButton = CreateButtonGO(buttonPrefab, "Cancel", "cancel");
        cancelButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            this.CloseDialog();
        });
    }

    private GameObject CreateButtonGO(UnityEngine.Object buttonPrefab, string name, string localizationCode)
    {
        GameObject buttonGameObject = (GameObject)Instantiate(buttonPrefab);
        buttonGameObject.transform.SetParent(this.transform.Find("Buttons").transform, false);
        buttonGameObject.name = "Button " + name;

        TextLocalizer textLocalizer = buttonGameObject.transform.GetComponentInChildren<TextLocalizer>();
        textLocalizer.formatValues = new string[] { LocalizationTable.GetLocalization(localizationCode) };
        textLocalizer.defaultText = localizationCode;

        return buttonGameObject;
    }
}
