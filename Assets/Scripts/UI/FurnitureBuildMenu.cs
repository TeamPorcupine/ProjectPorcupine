using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using ProjectPorcupine.Localization;

public class FurnitureBuildMenu : MonoBehaviour
{

    public GameObject buildFurnitureButtonPrefab;

    // Use this for initialization
    void Start()
    {

        BuildModeController bmc = GameObject.FindObjectOfType<BuildModeController>();
	
        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!


        foreach (string s in World.current.furniturePrototypes.Keys)
        {
            GameObject go = (GameObject)Instantiate(buildFurnitureButtonPrefab);
            go.transform.SetParent(this.transform);

            string objectId = s;
            string objectName = World.current.furniturePrototypes[s].Name;

            go.name = "Button - Build " + objectId;

            go.transform.GetComponentInChildren<Text>().text = "build";
            go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(World.current.furniturePrototypes[s].localizationCode) };

            Button b = go.GetComponent<Button>();

            b.onClick.AddListener(delegate
                {
                    bmc.SetMode_BuildFurniture(objectId);
                });

        }

    }
	
}
