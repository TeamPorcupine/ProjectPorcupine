using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuController : MonoBehaviour
{
    // The left build menu.
    public GameObject constructorMenu;

    // The sub menus of the build menu (furniture, floor..... later - power, security, drones).
    public GameObject furnitureMenu;
    public GameObject floorMenu;

    //The options and settings
    public GameObject optionsMenu;
    public GameObject settingsMenu;


    public Button buttonConstructor;
    public Button buttonWorld;
    public Button buttonWork;
    public Button buttonOptions;
    public Button buttonSettings;

    // Use this for initialization.
    void Start()
    {
        DeactivateAll();

        // Add liseners here.
        buttonConstructor.onClick.AddListener(delegate
            {
                OnButtonConstruction();
            });

        buttonWorld.onClick.AddListener(delegate
            {
                OnButtonWorld();
            });

        buttonWork.onClick.AddListener(delegate
            {
                OnButtonWork();
            });

        buttonOptions.onClick.AddListener(delegate
            {
                OnButtonOptions();
            });

        buttonSettings.onClick.AddListener(delegate
            {
                OnButtonSettings();
            });
    }

    // Deactivates All Menus.
    public void DeactivateAll()
    {
        constructorMenu.SetActive(false);
        settingsMenu.SetActive(false);
        optionsMenu.SetActive(false);
        DeactivateSubs();

    }

    // Deactivates any sub menu of the constrution options.
    public void DeactivateSubs()
    {
        furnitureMenu.SetActive(false);
        floorMenu.SetActive(false);
    }

    // Toggles whether menu is active.
    public void ToggleMenu(GameObject menu)
    {
        menu.SetActive(!menu.activeSelf);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            DeactivateAll();
        }
    }

    public void OnButtonConstruction()
    {
        DeactivateAll();
        constructorMenu.SetActive(true);
    }

    public void OnButtonWork()
    {
        DeactivateAll();

    }

    public void OnButtonWorld()
    {
        DeactivateAll();

    }

    public void OnButtonOptions()
    {
        DeactivateAll();
        optionsMenu.SetActive(true);
    }

    public void OnButtonSettings()
    {
        DeactivateAll();
        settingsMenu.SetActive(true);
    }
}