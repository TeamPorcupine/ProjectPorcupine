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
    public Image image;
    private string category;

    public void Initialize(string name)
    {
        text.text = name;
        this.category = name;
        button.onClick.AddListener(() => { SettingsMenu.DisplayCategory(category); });
    }

    public void Clicked()
    {
        image.color = new Color(1, 0.502f, 0.502f);
        text.color = new Color(0.89f, 0.392f, 0.4f);
    }

    public void UnClick()
    {
        image.color = Color.white;
        text.color = new Color(0.624f, 0.086f, 0.094f);
    }
}
