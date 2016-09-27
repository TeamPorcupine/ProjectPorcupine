using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private GameObject Canvas;

    public void Start()
    {
        Instance = this;
        Canvas = GameObject.Find("Canvas");
    }

    public void RenderMainMenu()
    {
        GameObject MainMenu = (GameObject)Instantiate(Resources.Load("UI/MainMenu"), Canvas.transform.position, Canvas.transform.rotation, Canvas.transform);
        MainMenu.SetActive(true);
    }
}
