#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

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
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Start()
    {
        HeadlineGenerator headlineGenerator = new HeadlineGenerator();
        headlineGenerator.UpdatedHeadline += UpdateHeadline;
        UpdateHeadline(headlineGenerator.CurrentDisplayText);
    }

    private void UpdateHeadline(string newHeadline)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        UnityDebugger.Debugger.Log("Headline", newHeadline);
        textBox.text = newHeadline;

        Scheduler.Scheduler.Current.DeregisterEvent(scheduledEvent);
        scheduledEvent = new ScheduledEvent(ToString(), (incomingEvent) => Dismiss(), dismissTime);
        scheduledEvent.IsSaveable = false;
        Scheduler.Scheduler.Current.RegisterEvent(scheduledEvent);
    }
}