#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;

public class MenuLeft : MonoBehaviour
{
    // This is the parent of the menus.
    private Transform parent;

    public GameObject CurrentlyOpen { get; private set; }

    // Use this for initialization
    public void Start()
    {
        parent = this.gameObject.transform;

        AddMenu("ConstructionMenu", "ConstructionMenu", typeof(ConstructionMenu));
        AddMenu("OrderMenu", "ConstructionMenu", typeof(OrderMenu));

        GameMenuManager.Instance.AddMenuItem("menu_construction", OnButtonConstruction, 0);
        GameMenuManager.Instance.AddMenuItem("menu_orders", OnButtonOrder, 1);
    }

    public void OpenMenu(string menuName)
    {
        GameObject menu = parent.FindChild(menuName).gameObject;

        CloseMenu();

        menu.SetActive(true);
        CurrentlyOpen = menu;

        WorldController.Instance.soundController.OnButtonSFX();

        if (CurrentlyOpen.name == "ConstructionMenu" || CurrentlyOpen.name == "OrderMenu")
        {
            WorldController.Instance.spawnInventoryController.SetUIVisibility(false);
        }
    }

    public void CloseMenu()
    {
        if (CurrentlyOpen != null)
        {
            CurrentlyOpen.SetActive(false);

            if (CurrentlyOpen.name == "ConstructionMenu" || CurrentlyOpen.name == "OrderMenu")
            {
                WorldController.Instance.spawnInventoryController.SetUIVisibility(SettingsKeyHolder.DeveloperMode);
            }

            WorldController.Instance.soundController.OnButtonSFX();

            CurrentlyOpen = null;
        }
    }

    // Use this function to add all the menus.
    private void AddMenu(string menuName, string prefabName, System.Type useComponent)
    {
        GameObject tempGoObj;
        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/MenuLeft/" + prefabName));
        tempGoObj.name = menuName;
        tempGoObj.transform.SetParent(parent, false);

        tempGoObj.AddComponent(useComponent);
    }

    private void OnButtonConstruction()
    {
        if (CurrentlyOpen != null && CurrentlyOpen.gameObject.name == "ConstructionMenu")
        {
            CloseMenu();
        }
        else
        {
            OpenMenu("ConstructionMenu");
        }
    }

    private void OnButtonOrder()
    {
        if (CurrentlyOpen != null && CurrentlyOpen.gameObject.name == "OrderMenu")
        {
            CloseMenu();
        }
        else
        {
            OpenMenu("OrderMenu");
        }
    }
}
