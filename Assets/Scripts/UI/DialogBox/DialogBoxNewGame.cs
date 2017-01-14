#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxNewGame : DialogBox
{
    public InputField Height;
    public InputField Width;
    public InputField Depth;
    public InputField Seed;
    public Toggle GenerateAsteroids;
    public InputField GeneratorInputField;
    public GameObject generatorListItemPrefab;
    public Transform generatorList;

    public override void ShowDialog()
    {
        base.ShowDialog();
        Height.onEndEdit.AddListener(delegate { VerifyNumericInput(Height); });
        Width.onEndEdit.AddListener(delegate { VerifyNumericInput(Width); });
        Depth.onEndEdit.AddListener(delegate { VerifyNumericInput(Depth); });

        // Generate Random Seed TODO: make it alphanumerical
        Seed.text = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString();

        // Get list of files in save location
        string generatorDirectoryPath = GameController.Instance.GeneratorBasePath();

        DirectoryInfo generatorDir = new DirectoryInfo(generatorDirectoryPath);

        FileInfo[] worldGenerators = generatorDir.GetFiles("*.xml").OrderBy(f => f.Name).ToArray();

        for (int i = 0; i < worldGenerators.Length; i++)
        {
            FileInfo file = worldGenerators[i];
            GameObject go = (GameObject)GameObject.Instantiate(generatorListItemPrefab);

            // Make sure this GameObject is a child of our list box
            go.transform.SetParent(generatorList);

            // file contains something like "C:\Users\UserName\......\Project Porcupine\Saves\SomeFileName.sav"
            // Path.GetFileName(file) returns "SomeFileName.sav"
            // Path.GetFileNameWithoutExtension(file) returns "SomeFileName"
            string fileName = Path.GetFileNameWithoutExtension(file.FullName);

            go.GetComponentInChildren<Text>().text = string.Format("{0}\n<size=11><i>{1}</i></size>", fileName, file.LastWriteTime);

            DialogListItem listItem = go.GetComponent<DialogListItem>();
            listItem.fileName = fileName;
            listItem.inputField = GeneratorInputField;
            listItem.currentColor = i % 2 == 0 ? ListPrimaryColor : ListSecondaryColor;

            go.GetComponent<Image>().color = listItem.currentColor;
        }

        // Set scroll sensitivity based on the save-item count
        generatorList.GetComponentInParent<ScrollRect>().scrollSensitivity = generatorList.childCount / 2;

        generatorList.GetComponent<AutomaticVerticalSize>().AdjustSize();
    }

    public void OkayWasClicked()
    {
        int height = int.Parse(Height.text);
        int width = int.Parse(Width.text);
        int depth = int.Parse(Depth.text);

        // Try and parse seed as Integer if this is not possible then hash string as integer
        int seed = 0;
        if (Seed.text == string.Empty)
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
        } 
        else if (int.TryParse(Seed.text, out seed) == false)
        {
            seed = Seed.text.GetHashCode();
            Debug.LogWarning("Converted " + Seed.text + " to hash " + seed);
        }

        string generatorFile = GeneratorInputField.text + ".xml";
        DialogBoxManager dialogManager = GameObject.FindObjectOfType<DialogBoxManager>();
        dialogManager.dialogBoxPromptOrInfo.SetPrompt("message_creating_new_world");
        dialogManager.dialogBoxPromptOrInfo.ShowDialog();
        SceneController.Instance.LoadNewWorld(width, height, depth, seed, generatorFile, GenerateAsteroids.isOn);
    }

    public void VerifyNumericInput(InputField input)
    {
        if (int.Parse(input.text) < 1)
        {
            input.text = "1";
        }
    }
}
