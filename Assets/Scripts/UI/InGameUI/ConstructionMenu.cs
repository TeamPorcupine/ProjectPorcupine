#region License
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

public class ConstructionMenu : MonoBehaviour
{
    // The sub menus of the build menu (furniture, floor..... later - power, security, drones).
    public GameObject furnitureMenu;
    public GameObject floorMenu;

    public Button buttonFloors;
    public Button buttonFurniture;
    public Button buttonDeconstruction;

    private BuildModeController bmc;

    public void OnClickDeconstruct()
    {
        DeactivateSubs();
        bmc.SetMode_Deconstruct();
    }

    public void OnClickFloors()
    {
        furnitureMenu.SetActive(false);
        ToggleMenu(floorMenu);
    }

    public void OnClickFurniture()
    {
        floorMenu.SetActive(false);
        ToggleMenu(furnitureMenu);
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

    private void Start()
    {
        bmc = WorldController.Instance.buildModeController;

        MenuController cm = GameObject.Find("MenuBottom").GetComponent<MenuController>();

        furnitureMenu = cm.furnitureMenu;
        floorMenu = cm.floorMenu;

        buttonDeconstruction.onClick.AddListener(delegate
        {
            OnClickDeconstruct();
        });

        // Add liseners here.
        buttonFloors.onClick.AddListener(delegate
        {
            OnClickFloors();
        });
        buttonFurniture.onClick.AddListener(delegate
        {
            OnClickFurniture();
        });
    }
}