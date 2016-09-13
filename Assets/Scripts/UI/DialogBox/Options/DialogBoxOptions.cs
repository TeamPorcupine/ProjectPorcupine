﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxOptions : DialogBox
{
    public Button buttonResume;
    public Button buttonNewWorld;
    public Button buttonSave;
    public Button buttonLoad;
    public Button buttonQuit;
    private DialogBoxManager dialogManager;

    public void OnButtonSaveGame()
    {
        this.CloseDialog();
        dialogManager.dialogBoxSaveGame.ShowDialog();
    }

    public void OnButtonLoadGame()
    {
        this.CloseDialog();
        dialogManager.dialogBoxLoadGame.ShowDialog();
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

    private void Start()
    {
        dialogManager = GameObject.FindObjectOfType<DialogBoxManager>();
        WorldController wc = GameObject.FindObjectOfType<WorldController>();

        // Add listeners here.
        buttonQuit.onClick.AddListener(delegate
        {
            OnButtonQuitGame();
        });
        buttonResume.onClick.AddListener(delegate
        {
            this.CloseDialog();
        });
        buttonNewWorld.onClick.AddListener(delegate
        {
            wc.NewWorld();
        });

        buttonSave.onClick.AddListener(delegate
        {
            OnButtonSaveGame();
        });
        buttonLoad.onClick.AddListener(delegate
        {
            OnButtonLoadGame();
        });
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            this.CloseDialog();
        }
    }
}