using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {

    public GameObject furnitureMenu;

    public void ToggleFurnitureMenu() {
        bool toggle = !furnitureMenu.activeInHierarchy;
        furnitureMenu.SetActive(toggle);
    }
}
