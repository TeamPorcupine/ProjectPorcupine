using System.Collections.Generic;

public interface IContextActionProvider
{
    IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu);
}