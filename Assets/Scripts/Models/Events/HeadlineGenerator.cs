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
    private const float MinInterval = 50f;
    private const float MaxInterval = 100f;

    private List<string> headlines;

    private ScheduledEvent scheduledEvent;

    public HeadlineGenerator()
    {
        headlines = new List<string>();

        foreach (Headline headline in PrototypeManager.Headline.Values)
        {
            headlines.Add(headline.Text);
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
        float nextTime = UnityEngine.Random.Range(MinInterval, MaxInterval);
        scheduledEvent = new ScheduledEvent(ToString(), (incomingEvent) => OnUpdatedHeadline(), nextTime);
        Scheduler.Scheduler.Current.RegisterEvent(scheduledEvent);
    }
}
