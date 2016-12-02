using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Just a simple wrapper.
/// </summary>
public class CategoryButtonHandler : MonoBehaviour
{
    public Text text;
    public Button button;
    private string category;

    public void Initialize(string name)
    {
        text.text = name;
        this.category = name;
        button.onClick.AddListener(() => { SettingsMenu.DisplayCategory(category); });
    }
}
