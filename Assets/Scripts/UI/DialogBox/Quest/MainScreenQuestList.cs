using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainScreenQuestList : MonoBehaviour
{
    private float totalDeltaTime;
    private readonly float checkDelayInSeconds;
    public Transform QuestItemListPanel;
    public GameObject QuestItemPrefab;
    private List<Quest> visibleQuests;

    public MainScreenQuestList()
    {
        checkDelayInSeconds = .5f;
        visibleQuests = new List<Quest>();
    }

    public void Update()
    {
        totalDeltaTime += Time.deltaTime;
        if (totalDeltaTime >= checkDelayInSeconds)
        {
            totalDeltaTime = 0f;
            RefreshInterface();
        }
    }

    private void RefreshInterface()
    {
        ClearInterface();
        BuildInterface();
    }
    
    private void ClearInterface()
    {
        List<Quest> quests = World.current.Quests.Where(q => q.IsAccepted && !q.IsCompleted).ToList();
        var childrens = QuestItemListPanel.Cast<Transform>().ToList();
        foreach (var child in childrens)
        {
            DialogBoxQuestItem qi = child.GetComponent<DialogBoxQuestItem>();
            if (!quests.Contains(qi.Quest))
            {
                visibleQuests.Remove(qi.Quest);
                Destroy(child.gameObject);
            }
        }
    }

    private void BuildInterface()
    {
        List<Quest> quests = World.current.Quests.Where(q => q.IsAccepted && !q.IsCompleted).ToList();

        foreach (var quest in quests)
        {
            if (!visibleQuests.Contains(quest))
            {
                var go = (GameObject) Instantiate(QuestItemPrefab);
                go.transform.SetParent(QuestItemListPanel);

                var questItemBehaviour = go.GetComponent<DialogBoxQuestItem>();
                questItemBehaviour.SetupQuest(quest);
                visibleQuests.Add(quest);
            }
        }
    }
}