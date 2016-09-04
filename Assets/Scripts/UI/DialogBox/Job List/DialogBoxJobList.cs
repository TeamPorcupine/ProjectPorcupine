#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

////   Object -> MonoBehaviour -> DialogBox -> DialogBoxLoadSaveGame ->
////                                                        DialogBoxSaveGame
////                                                        DialogBoxLoadGame

public class DialogBoxJobList : DialogBox
{
    public static readonly Color SecondaryColor = new Color(0.9f, 0.9f, 0.9f);

    public GameObject jobListItemPrefab;
    public Transform jobList;

    public override void ShowDialog()
    {
        base.ShowDialog();
        
        foreach (Character c in World.Current.characters)
        {
            GameObject go = (GameObject)Instantiate(jobListItemPrefab, jobList);
            go.GetComponentInChildren<Text>().text = c.GetName() + " - " + c.GetJobDescription();
            
        }

        jobList.GetComponentInParent<ScrollRect>().scrollSensitivity = jobList.childCount / 2;
    }

    public override void CloseDialog()
    {
        // Clear out all the children of our file list
        while (jobList.childCount > 0)
        {
            Transform c = jobList.GetChild(0);
            c.SetParent(null);  // Become Batman
            Destroy(c.gameObject);
        }

        base.CloseDialog();
    }
}
