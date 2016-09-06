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
using System.Xml;
using Scheduler;
using UnityEngine;
using UnityEngine.UI;

public class HeadlineController : MonoBehaviour
{
    public Text textBox;
    public float dismissTime = 10;

    private ScheduledEvent scheduledEvent;
    private CanvasGroup canvasGroup;

    public void Dismiss()
    {
        if (canvasGroup==null)
        {
            return;
        }
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        string filePath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "Headlines.xml");
        string xmlText = System.IO.File.ReadAllText(filePath);
        XmlDocument doc = new XmlDocument();
        doc.Load(new StringReader(xmlText));
        HeadlineGenerator headlineGenerator = new HeadlineGenerator(doc.SelectSingleNode("Headlines"));
        headlineGenerator.UpdatedHeadline += UpdateHeadline;
        UpdateHeadline(headlineGenerator.CurrentDisplayText);
    }

    private void UpdateHeadline(string newHeadline)
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        Debug.ULogChannel("Headline", newHeadline);
        textBox.text = newHeadline;

        Scheduler.Scheduler.Current.DeregisterEvent(scheduledEvent);
        scheduledEvent = new ScheduledEvent(ToString(), (incomingEvent) => Dismiss(), dismissTime);
        Scheduler.Scheduler.Current.RegisterEvent(scheduledEvent);
    }
}