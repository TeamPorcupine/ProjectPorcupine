#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using Scheduler;

public class HeadlineGenerator 
{
    private List<string> headlines = new List<string>();
    private float minInterval, maxInterval;
    private ScheduledEvent scheduledEvent;

    public HeadlineGenerator(XmlNode baseNode)
    {
        // TODO Consider default values for these. Also consider reasonable limits
        minInterval = float.Parse(baseNode.Attributes.GetNamedItem("minInterval").Value);
        maxInterval = float.Parse(baseNode.Attributes.GetNamedItem("maxInterval").Value);
        foreach (XmlNode node in baseNode.SelectNodes("Headline"))
        {
            headlines.Add(node.InnerText);
        }

        OnUpdatedHeadline();
        ResetNextTime();
    }

    public event Action<string> UpdatedHeadline;

    public string CurrentDisplayText { get; protected set; }

    public void AddHeadline(string headline, bool displayImmediately = true, bool keepInQueue = true)
    {
        if (keepInQueue)
        {
            headlines.Add(headline);
        }

        if (displayImmediately)
        {
            OnUpdatedHeadline(headline);
        }
    }

    private void OnUpdatedHeadline()
    {
        OnUpdatedHeadline(headlines[UnityEngine.Random.Range(0, headlines.Count)]);
    }

    private void OnUpdatedHeadline(string headline)
    {
        CurrentDisplayText = headline;
        Action<string> handler = UpdatedHeadline;
        if (handler != null)
        {
            handler(headline);
        }

        ResetNextTime();
    }

    private void ResetNextTime()
    {
        Scheduler.Scheduler.Current.DeregisterEvent(scheduledEvent);
        float nextTime = UnityEngine.Random.Range(minInterval, maxInterval);
        scheduledEvent = new ScheduledEvent(ToString(), (incomingEvent) => OnUpdatedHeadline(), nextTime);
        Scheduler.Scheduler.Current.RegisterEvent(scheduledEvent);
    }
}
