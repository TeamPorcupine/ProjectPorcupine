using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConstructionMenu : MonoBehaviour
{

    // The sub menus of the build menu (furniture, floor..... later - power, security, drones).
    public GameObject furnitureMenu;
    public GameObject floorMenu;

    public Button buttonFloors;
    public Button buttonFurniture;

    void OnEnable()
    {
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