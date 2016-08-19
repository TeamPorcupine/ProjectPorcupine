using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour 
{
    public GameObject loadFile;
    public GameObject saveFile;

	// Use this for initialization
	void Start () 
    {
        //Set's up the top menu at start




        GameObject canvas = GameObject.Find("Canvas");
        GameObject topMenu = Instantiate(Resources.Load("TopMenu"), canvas.transform) as GameObject;
        topMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        GameObject newWorld = Instantiate(Resources.Load("Button - New World"), topMenu.transform) as GameObject;
        GameObject load = Instantiate(Resources.Load("Button - Load"), topMenu.transform) as GameObject;
        GameObject save = Instantiate(Resources.Load("Button - Save"), topMenu.transform) as GameObject;
        GameObject pathTest = Instantiate(Resources.Load("Button - Pathfinding Test"), topMenu.transform) as GameObject;

        var worldScript = GameObject.Find("WorldController").GetComponent<WorldController>();
        var loadScript = loadFile.GetComponent<DialogBoxLoadGame>();
        var saveScript = saveFile.GetComponent<DialogBoxSaveGame>();
        var buildScript = GameObject.Find("BuildModeController").GetComponent<BuildModeController>();

        newWorld.GetComponent<Button>().onClick.AddListener(() => worldScript.NewWorld());
        load.GetComponent<Button>().onClick.AddListener(() => loadScript.ShowDialog());
        save.GetComponent<Button>().onClick.AddListener(() => saveScript.ShowDialog());
        pathTest.GetComponent<Button>().onClick.AddListener(() => buildScript.DoPathfindingTest());
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}
}
