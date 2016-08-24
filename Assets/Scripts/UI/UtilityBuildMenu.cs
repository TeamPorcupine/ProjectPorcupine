#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Linq;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;
public class UtilityBuildMenu : MonoBehaviour
{
    public GameObject buildUtilityButtonPrefab;

    private string lastLanguage;

    // Use this for initialization
    private void Start()
    {
        BuildModeController bmc = WorldController.Instance.buildModeController;

        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!
        foreach (string s in World.current.utilityPrototypes.Keys)
        {
            GameObject go = (GameObject)Instantiate(buildUtilityButtonPrefab);
            go.transform.SetParent(this.transform);

            string objectId = s;
            string objectName = World.current.utilityPrototypes[s].Name;

            go.name = "Button - Build " + objectId;

            go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(World.current.utilityPrototypes[s].localizationCode) };

            Button b = go.GetComponent<Button>();

            b.onClick.AddListener(delegate
                {
                    bmc.SetMode_BuildUtility(objectId);
                    this.gameObject.SetActive(false);
                });

            // http://stackoverflow.com/questions/1757112/anonymous-c-sharp-delegate-within-a-loop
            string util = s;
            LocalizationTable.CBLocalizationFilesChanged += delegate
            {
                go.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(World.current.utilityPrototypes[util].localizationCode) };
            };
        }

        lastLanguage = LocalizationTable.currentLanguage;
    }

    private void Update()
    {
        if (lastLanguage != LocalizationTable.currentLanguage)
        {
            lastLanguage = LocalizationTable.currentLanguage;

            TextLocalizer[] localizers = GetComponentsInChildren<TextLocalizer>();

            for (int i = 0; i < localizers.Length; i++)
            {
                localizers[i].UpdateText(LocalizationTable.GetLocalization(World.current.utilityPrototypes.ElementAt(i).Value.GetName()));
            }
        }
    }
}
