#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConstructionMenu : MonoBehaviour
{

    // The sub menus of the build menu (furniture, floor..... later - power, security, drones).
    public GameObject furnitureMenu;
    public GameObject floorMenu;

    BuildModeController bmc;

    public Button buttonFloors;
    public Button buttonFurniture;

    public Button buttonDeconstruction;

    void Start()
    {
        bmc = WorldController.Instance.buildModeController;

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

    public void OnClickDeconstruct()
    {
        DeactivateSubs();
        bmc.SetMode_Deconstruct();
    }

    public void OnClickFloors()
    {
        DeactivateSubs();
        ToggleMenu(floorMenu);
    }

    public void OnClickFurniture()
    {
        DeactivateSubs();
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
}