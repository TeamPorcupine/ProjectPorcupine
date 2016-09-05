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
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxJobList : DialogBox
{
    public GameObject JobListItemPrefab;

    public Transform JobList;

    // These are used as an update period to keep the list updated, and avoid spam
    // (We already have a lot of FPS issues...)
    // Also, it seems that if this "sleep" period isn't here, you wouldn't be able to
    // click on the delete button, as well as it keeps those buttons well aligned (try
    // to remove it and you'll get what I mean).
    private int waitPeriod = 5;

    private int currentWait = 0;

    public override void ShowDialog()
    {
        gameObject.SetActive(true);
    }

    public override void CloseDialog()
    {
        // Clear out all the children of our file list
        while (JobList.childCount > 0)
        {
            Transform c = JobList.GetChild(0);
            c.SetParent(null);  // Become Batman
            Destroy(c.gameObject);
        }

        base.CloseDialog();
    }

    private void Update()
    {
        if (currentWait == 5)
        {
            currentWait = 0;
            while (JobList.childCount > 0)
            {
                Transform c = JobList.GetChild(0);
                c.SetParent(null);
                Destroy(c.gameObject);
            }

            // Localization
            string[] formatValues;
            formatValues = new string[0];
            foreach (Character c in World.Current.characters)
            {
                GameObject go = (GameObject)Instantiate(JobListItemPrefab, JobList);
                go.GetComponentInChildren<Text>().text = c.GetName() + " - " + LocalizationTable.GetLocalization(c.GetJobDescription(), formatValues);
            }

            JobList.GetComponentInParent<ScrollRect>().scrollSensitivity = JobList.childCount / 2;
        }
        else
        {
            currentWait += 1;
        }
    }
}
