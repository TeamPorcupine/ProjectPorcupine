using UnityEngine;
using System.Collections;

public class GUIHelper : MonoBehaviour
{
    public void ToggleActive(GameObject go)
    {
        go.SetActive(!go.activeSelf);
    }
}
