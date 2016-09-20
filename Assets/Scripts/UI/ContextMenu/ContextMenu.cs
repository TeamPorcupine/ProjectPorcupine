#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContextMenu : MonoBehaviour
{
    public GameObject ContextualMenuItemPrefab;

    /// <summary>
    /// Open the context menu at the specified tile.
    /// </summary>
    /// <param name="tile">The Tile where the context menu is to be opened.</param>
    public void Open(Tile tile)
    {
        gameObject.SetActive(true);

        List<IContextActionProvider> providers = GetContextualActionProviderOnTile(tile);
        List<ContextMenuAction> contextActions = GetContextualMenuActionFromProviders(providers);

        ClearInterface();
        BuildInterface(contextActions);
    }

    /// <summary>
    /// Close this context menu.
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Clears the interface.
    /// </summary>
    private void ClearInterface()
    {
        List<Transform> childrens = gameObject.transform.Cast<Transform>().ToList();
        foreach (Transform child in childrens)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Builds the interface.
    /// </summary>
    /// <param name="contextualActions">Contextual actions.</param>
    private void BuildInterface(List<ContextMenuAction> contextualActions)
    {
        gameObject.transform.position = Input.mousePosition + new Vector3(10, -10, 0);

        bool characterSelected = WorldController.Instance.mouseController.IsCharacterSelected();

        foreach (ContextMenuAction contextMenuAction in contextualActions)
        {
            if ((contextMenuAction.RequireCharacterSelected && characterSelected) ||
                !contextMenuAction.RequireCharacterSelected)
            {
                GameObject go = (GameObject)Instantiate(ContextualMenuItemPrefab);
                go.transform.SetParent(gameObject.transform);

                ContextMenuItem contextMenuItem = go.GetComponent<ContextMenuItem>();
                contextMenuItem.ContextMenu = this;
                contextMenuItem.Action = contextMenuAction;
                contextMenuItem.BuildInterface();
            }
        }
    }

    /// <summary>
    /// Gets the contextual action provider for the specified tile.
    /// </summary>
    /// <returns>The contextual action provider for the specified tile.</returns>
    /// <param name="tile">Tile to get a contextual action provider for.</param>
    private List<IContextActionProvider> GetContextualActionProviderOnTile(Tile tile)
    {
        List<IContextActionProvider> providers = new List<IContextActionProvider>();

        providers.Add(tile);

        if (tile.Furniture != null)
        {
            providers.Add(tile.Furniture);
        }

        if (tile.Utilities != null)
        {
            foreach (Utility utility in tile.Utilities.Values)
            {
                providers.Add(utility);
            }
        }

        if (tile.Characters != null)
        {
            foreach (Character character in tile.Characters)
            {
                providers.Add(character);
            }
        }

        if (tile.Inventory != null)
        {
            providers.Add(tile.Inventory);
        }

        return providers;
    }

    /// <summary>
    /// Gets the contextual menu action from a list of providers.
    /// </summary>
    /// <returns>The contextual menu action from a list of providers.</returns>
    /// <param name="providers">Providers.</param>
    private List<ContextMenuAction> GetContextualMenuActionFromProviders(List<IContextActionProvider> providers)
    {
        List<ContextMenuAction> contextualActions = new List<ContextMenuAction>();
        foreach (IContextActionProvider contextualActionProvider in providers)
        {
            contextualActions.AddRange(contextualActionProvider.GetContextMenuActions(this));
        }
        
        return contextualActions;
    }
}
