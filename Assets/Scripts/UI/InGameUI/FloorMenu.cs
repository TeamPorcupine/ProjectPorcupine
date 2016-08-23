#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ProjectPorcupine.Localization;

public class FloorMenu : MonoBehaviour
{

    public GameObject buildFurnitureButtonPrefab;

    string lastLanguage;

    // A chached copy to save gc. Could result in issues if TileType gets dynamically changed.
    private TileType[] tileTypes;

    //public Button floorBuild;
    //public Button ladderBuild;
    //public Button floorRemove;

    // Use this for initialization.
    void Start()
    {

        tileTypes = TileType.GetTileTypes();

        BuildModeController bmc = WorldController.Instance.buildModeController;

        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!
        
        foreach (TileType type in tileTypes)
        {
            GameObject go = (GameObject)Instantiate(buildFurnitureButtonPrefab);
            go.transform.SetParent(this.transform);

            TileType tileType = type;

            go.name = "Button - Build " + tileType.Type;

            // TODO: Not a elegant solution! Find a better way.
            if (type == TileType.Empty)
            {
                go.name = "Button - Remove " + tileType.Type;

                go.GetComponentInChildren<TextLocalizer>().defaultText = "remove";
            }

            go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(tileType.LocalizationCode) };

            Button b = go.GetComponent<Button>();

            b.onClick.AddListener(delegate
            {
                bmc.SetModeBuildTile(tileType);
            });

        }

        lastLanguage = LocalizationTable.currentLanguage;
    }

    // Update is called once per frame.
    void Update()
    {
        if (lastLanguage != LocalizationTable.currentLanguage)
        {
            lastLanguage = LocalizationTable.currentLanguage;

            TextLocalizer[] localizers = GetComponentsInChildren<TextLocalizer>();

            for (int i = 0; i < localizers.Length; i++)
            {
                localizers[i].UpdateText(LocalizationTable.GetLocalization(tileTypes[i].LocalizationCode));
            }
        }
    }
}