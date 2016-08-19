using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PanelButton : MonoBehaviour
{
    private bool open = false;

    // Open the flyout
    public void Open()
    {
        this.gameObject.GetComponentInParent<PanelController>().CloseAllFlyouts(this);
        open = true;
        UpdateState();
    }

    // Close the flyout
    public void Close()
    {
        open = false;
        UpdateState();
    }

    // Toggles the flyout
    public void Toggle()
    {
        open = !open;
        if (open)
            this.gameObject.GetComponentInParent<PanelController>().CloseAllFlyouts(this);
        UpdateState();
    }

    private void UpdateState()
    {
        this.gameObject.GetComponentInChildren<PanelFlyout>(true).gameObject.SetActive(open);
    }
}
