using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PanelController : MonoBehaviour
{
    public void CloseAllFlyouts(PanelButton except)
    {
        foreach (var button in this.GetComponentsInChildren<PanelButton>(true))
        {
            if (except != button)
                button.Close();
        }
    }
}
