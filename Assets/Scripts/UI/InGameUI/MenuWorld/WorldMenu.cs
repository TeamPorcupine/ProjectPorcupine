using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldMenu : MonoBehaviour
{
    public Text worldName;

    void OnEnable()
    {
        worldName.text = World.Current.name;
    }
}
