#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using ProjectPorcupine.Localization;

public class FurnitureBuildMenu : MonoBehaviour
{

    public GameObject buildFurnitureButtonPrefab;

    string lastLanguage;

    // Use this for initialization
    void Start()
    {

        BuildModeController bmc = WorldController.Instance.buildModeController;
	
        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!


        foreach (string s in World.current.furniturePrototypes.Keys)
        {
            GameObject go = (GameObject)Instantiate(buildFurnitureButtonPrefab);
            go.transform.SetParent(this.transform);

            string objectId = s;
            string objectName = World.current.furniturePrototypes[s].Name;

            go.name = "Button - Build " + objectId;
            
            go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(World.current.furniturePrototypes[s].localizationCode) };

            Button b = go.GetComponent<Button>();

            b.onClick.AddListener(delegate
                {
                    bmc.SetMode_BuildFurniture(objectId);
                });

        }

        lastLanguage = LocalizationTable.currentLanguage;

    }

    void Update()
    {
        if(lastLanguage != LocalizationTable.currentLanguage)
        {
            lastLanguage = LocalizationTable.currentLanguage;

            TextLocalizer[] localizers = GetComponentsInChildren<TextLocalizer>();

            for(int i = 0; i < localizers.Length; i++)
            {
                localizers[i].UpdateText(LocalizationTable.GetLocalization(World.current.furniturePrototypes.ElementAt(i).Value.GetName()));
            }
        }
    }
	
}
