#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class SettingsHeading : MonoBehaviour
{
    [SerializeField]
    private GameObject root;
    [SerializeField]
    private Text headingText;

    public void SetText(string text)
    {
        headingText.text = LocalizationTable.GetLocalization(text);
    }

    public void AddObjectToRoot(GameObject go)
    {
        go.transform.SetParent(root.transform);
    }

    public void RemoveObjectsFromRoot()
    {
        foreach (Transform child in root.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
