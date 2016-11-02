#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainScreenQuestList : MonoBehaviour
{
    public Transform QuestItemListPanel;
    public GameObject QuestItemPrefab;

    private readonly float checkDelayInSeconds;
    private float totalDeltaTime;
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
        List<Quest> quests = PrototypeManager.Quest.Values.Where(q => q.IsAccepted && !q.IsCompleted).ToList();
        List<Transform> childrens = QuestItemListPanel.Cast<Transform>().ToList();
        foreach (Transform child in childrens)
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
        List<Quest> quests = PrototypeManager.Quest.Values.Where(q => q.IsAccepted && !q.IsCompleted).ToList();

        foreach (Quest quest in quests)
        {
            if (!visibleQuests.Contains(quest))
            {
                GameObject go = (GameObject)Instantiate(QuestItemPrefab);
                go.transform.SetParent(QuestItemListPanel);

                DialogBoxQuestItem questItemBehaviour = go.GetComponent<DialogBoxQuestItem>();
                questItemBehaviour.SetupQuest(quest);
                visibleQuests.Add(quest);
            }
        }
    }
}