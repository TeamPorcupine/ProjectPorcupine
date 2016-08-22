using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContextMenu : MonoBehaviour
{
    public GameObject ContextualMenuItemPrefab;

    public void Open(Tile tile)
    {
        gameObject.SetActive(true);

        var providers = GetContextualActionProviderOnTile(tile);
        var contextActions = GetContextualMenuActionFromProviders(providers);

        ClearInterface();
        BuildInterface(contextActions);
    }
    
    private void ClearInterface()
    {
        var childrens = gameObject.transform.Cast<Transform>().ToList();
        foreach (var child in childrens)
        {
            Destroy(child.gameObject);
        }
    }

    private void BuildInterface(List<ContextMenuAction> contextualActions)
    {
        gameObject.transform.position = Input.mousePosition + new Vector3(10, -10, 0);

        foreach (var contextMenuAction in contextualActions)
        {
            var go = (GameObject)Instantiate(ContextualMenuItemPrefab);
            go.transform.SetParent(gameObject.transform);


            var contextMenuItem = go.GetComponent<ContextMenuItem>();
            contextMenuItem.ContextMenu = this;
            contextMenuItem.Action = contextMenuAction;
            contextMenuItem.BuildInterface();
        }
    }

    private List<IContextActionProvider> GetContextualActionProviderOnTile(Tile tile)
    {
        var providers = new List<IContextActionProvider>();

        if (tile.Furniture != null)
        {
            providers.Add(tile.Furniture);
        }

        if (tile.Characters != null)
        {
            foreach (var character in tile.Characters)
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

    private List<ContextMenuAction> GetContextualMenuActionFromProviders(List<IContextActionProvider> providers)
    {
        var contextualActions = new List<ContextMenuAction>();
        foreach (var contextualActionProvider in providers)
        {
            contextualActions.AddRange(contextualActionProvider.GetContextMenuActions(this));
        }

        contextualActions = contextualActions.OrderBy(c => c.Text).ToList();
        return contextualActions;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}