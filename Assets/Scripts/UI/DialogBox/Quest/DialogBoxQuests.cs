﻿#region License
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

public class DialogBoxQuests : DialogBox
{
    public Transform QuestItemListPanel;
    public GameObject QuestItemPrefab;

    public override void ShowDialog()
    {
        base.ShowDialog();

        ClearInterface();
        BuildInterface();
    }

    private void ClearInterface()
    {
        var childrens = QuestItemListPanel.Cast<Transform>().ToList();
        foreach (var child in childrens)
        {
            Destroy(child.gameObject);
        }
    }

    private void BuildInterface()
    {
        List<Quest> quests = World.Current.Quests.Where(q => IsQuestAvailable(q)).ToList();

        foreach (var quest in quests)
        {
            var go = (GameObject)Instantiate(QuestItemPrefab);
            go.transform.SetParent(QuestItemListPanel);

            var questItemBehaviour = go.GetComponent<DialogBoxQuestItem>();
            questItemBehaviour.SetupQuest(this, quest);
        }
    }

    private bool IsQuestAvailable(Quest quest)
    {
        if (quest.IsAccepted)
        {
            return false;
        }

        if (quest.PreRequiredCompletedQuest.Count == 0)
        {
            return true;
        }

        List<Quest> preQuests = World.Current.Quests.Where(q => quest.PreRequiredCompletedQuest.Contains(q.Name)).ToList();

        return preQuests.All(q => q.IsCompleted);
    }
}