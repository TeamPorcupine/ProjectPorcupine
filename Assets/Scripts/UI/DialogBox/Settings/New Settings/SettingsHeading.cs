using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SettingsHeading : MonoBehaviour
{
    [SerializeField]
    GameObject root;
    [SerializeField]
    Text headingText;

    public void SetText(string text)
    {
        headingText.text = text;
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
