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

public class ConstructionMenu : MonoBehaviour
{
    // The sub menus of the build menu (furniture, floor..... later - power, security, drones).
    public GameObject furnitureMenu;
    public GameObject floorMenu;

    public Button buttonFloors;
    public Button buttonFurniture;
    public Button buttonDeconstruction;

    private BuildModeController bmc;

    private GameObject[] FurnitureSubs 
    {
        get 
        {
            return new GameObject[] 
            {
                // add every furniture submenu here
                furnitureMenu, floorMenu
            };
        }
    }

    public void OnClickDeconstruct()
    {
        DeactivateSubs();
        bmc.SetMode_Deconstruct();
    }

    public void OnClickFloors()
    {
        DeactivateSubsExcept(floorMenu);
        ToggleMenu(floorMenu);
    }

    public void OnClickFurniture()
    {
        DeactivateSubsExcept(furnitureMenu);
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

    public void DeactivateSubsExcept(GameObject menu)
    {
        foreach (GameObject g in FurnitureSubs)
        {
            if (g == menu)
            {
                continue;
            }

            g.SetActive(false);
        }
    }

    private void Start()
    {
        bmc = WorldController.Instance.buildModeController;

        MenuController cm = GameObject.Find("MenuBottom").GetComponent<MenuController>();

        furnitureMenu = cm.furnitureMenu;
        floorMenu = cm.floorMenu;

        // Add liseners here.
        buttonDeconstruction.onClick.AddListener(delegate
        {
            OnClickDeconstruct();
        });

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