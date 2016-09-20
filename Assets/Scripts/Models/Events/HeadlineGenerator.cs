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

    private Queue<Headline> headlinesQueue;

    private ScheduledEvent scheduledEvent;
    private Random random;

    public HeadlineGenerator()
    {
        random = new System.Random();

        headlinesQueue = new Queue<Headline>();

        OnUpdatedHeadline();
        ResetNextTime();
    }

    public event Action<string> UpdatedHeadline;

    public string CurrentDisplayText { get; protected set; }

    public void AddHeadline(string headline, bool displayImmediately = true)
    {
        if (displayImmediately)
        {
            OnUpdatedHeadline(headline);
        }
        else
        {
            headlinesQueue.Enqueue(new Headline(headline));
        }
    }

    private void OnUpdatedHeadline()
    {
        int index = random.Next(0, PrototypeManager.Headline.Count + headlinesQueue.Count);
        string text;

        if (index > PrototypeManager.Headline.Count)
        {
            Headline headline = headlinesQueue.Dequeue();
            text = headline.Text;
        }
        else
        {
            text = PrototypeManager.Headline[index].Text;
        }

        OnUpdatedHeadline(text);
    }

    private void OnUpdatedHeadline(string headline)
    {
        CurrentDisplayText = headline;

        if (UpdatedHeadline != null)
        {
            UpdatedHeadline(headline);
        }

        ResetNextTime();
    }

    private void ResetNextTime()
    {
        double range = (double)MaxInterval - (double)MinInterval;
        float nextTime = (float)((range * random.NextDouble()) + (double)MinInterval);
        Scheduler.Scheduler.Current.DeregisterEvent(scheduledEvent);
        scheduledEvent = new ScheduledEvent(ToString(), (incomingEvent) => OnUpdatedHeadline(), nextTime);
        Scheduler.Scheduler.Current.RegisterEvent(scheduledEvent);
    }
}
