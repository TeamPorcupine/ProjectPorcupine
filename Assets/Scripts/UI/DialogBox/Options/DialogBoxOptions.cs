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

public class DialogBoxOptions : DialogBox
{
    private DialogBoxManager dialogManager;
    private bool cancel;

    public void OnButtonNewWorld()
    {
        StartCoroutine(OnButtonNewWorldCoroutine());
    }

    public void OnButtonSaveGame()
    {
        this.CloseDialog();
        dialogManager.dialogBoxSaveGame.ShowDialog();
    }

    public void OnButtonLoadGame()
    {
        StartCoroutine(OnButtonLoadGameCoroutine());
    }

    public void OnButtonOpenSettings()
    {
        this.CloseDialog();
        dialogManager.dialogBoxSettings.ShowDialog();
    }

    // Quit the app whether in editor or a build version.
    public void OnButtonQuitGame()
    {
        StartCoroutine(ConfirmQuitDialog());
    }

    private IEnumerator ConfirmQuitDialog()
    {
        dialogManager.dialogBoxPromptOrInfo.SetPrompt("prompt_confirm_quit");
        dialogManager.dialogBoxPromptOrInfo.SetButtons(DialogBoxResult.Yes, DialogBoxResult.No);

        dialogManager.dialogBoxPromptOrInfo.Closed = () =>
        {
            if (dialogManager.dialogBoxPromptOrInfo.Result == DialogBoxResult.Yes)
            {
                // Quit the game
#if UNITY_EDITOR
                // Allows you to quit in the editor.
                UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
            }
        };

        dialogManager.dialogBoxPromptOrInfo.ShowDialog();

        while (dialogManager.dialogBoxPromptOrInfo.gameObject.activeSelf)
        {
            yield return null;
        }
    }

    private IEnumerator CheckIfSaveGameBefore(string prompt)
    {
        bool saveGame = false;
        cancel = false;

        dialogManager.dialogBoxPromptOrInfo.SetPrompt(prompt);
        dialogManager.dialogBoxPromptOrInfo.SetButtons(DialogBoxResult.Yes, DialogBoxResult.No, DialogBoxResult.Cancel);

        dialogManager.dialogBoxPromptOrInfo.Closed = () =>
        {
            if (dialogManager.dialogBoxPromptOrInfo.Result == DialogBoxResult.Yes)
            {
                saveGame = true;
            }

            if (dialogManager.dialogBoxPromptOrInfo.Result == DialogBoxResult.Cancel)
            {
                cancel = true;
            }
        };

        dialogManager.dialogBoxPromptOrInfo.ShowDialog();

        while (dialogManager.dialogBoxPromptOrInfo.gameObject.activeSelf)
        {
            yield return null;
        }

        if (saveGame)
        {
            dialogManager.dialogBoxSaveGame.ShowDialog();
        }
    }

    private IEnumerator OnButtonNewWorldCoroutine()
    {
        StartCoroutine(CheckIfSaveGameBefore("prompt_save_before_creating_new_world"));

        while (dialogManager.dialogBoxSaveGame.gameObject.activeSelf || dialogManager.dialogBoxPromptOrInfo.gameObject.activeSelf)
        {
            yield return null;
        }

        if (!cancel)
        {
            this.CloseDialog();
            dialogManager.dialogBoxPromptOrInfo.SetPrompt("message_creating_new_world");
            dialogManager.dialogBoxPromptOrInfo.ShowDialog();

            WorldController.Instance.LoadWorld(null);
        }
    }

    private IEnumerator OnButtonLoadGameCoroutine()
    {
        StartCoroutine(CheckIfSaveGameBefore("prompt_save_before_loading_new_game"));

        while (dialogManager.dialogBoxSaveGame.gameObject.activeSelf || dialogManager.dialogBoxPromptOrInfo.gameObject.activeSelf)
        {
            yield return null;
        }

        if (!cancel)
        {
            this.CloseDialog();
            dialogManager.dialogBoxLoadGame.ShowDialog();
        }
    }

    private void RenderButtons()
    {
        UnityEngine.Object buttonPrefab = Resources.Load("UI/Components/MenuButton");

        GameObject resumeButton = CreateButtonGO(buttonPrefab, "Resume", "menu_resume");
        resumeButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            this.CloseDialog();
        });

        GameObject newWorldButton = CreateButtonGO(buttonPrefab, "New World", "new_world");
        newWorldButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnButtonNewWorld();
        });

        GameObject saveButton = CreateButtonGO(buttonPrefab, "Save", "save");
        saveButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnButtonSaveGame();
        });

        GameObject loadButton = CreateButtonGO(buttonPrefab, "Load", "load");
        loadButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnButtonLoadGame();
        });

        GameObject settingsButton = CreateButtonGO(buttonPrefab, "Settings", "menu_settings");
        settingsButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnButtonOpenSettings();
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

    private void Start()
    {
        dialogManager = GameObject.FindObjectOfType<DialogBoxManager>();

        RenderButtons();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            this.CloseDialog();
        }
    }
}
