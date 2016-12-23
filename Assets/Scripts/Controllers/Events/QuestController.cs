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
using System.Linq;
using UnityEngine;

public class QuestController
{
    private readonly float checkDelayInSeconds;

    private float totalDeltaTime;

    public QuestController()
    {
        checkDelayInSeconds = 5f;

        TimeManager.Instance.FixedFrequencyUnpaused += Update;
    }

    public void Update(float deltaTime)
    {
        totalDeltaTime += deltaTime;
        if (totalDeltaTime >= checkDelayInSeconds)
        {
            totalDeltaTime = 0f;
            CheckAllAcceptedQuests();
        }
    }

    private void CheckAllAcceptedQuests()
    {
        List<Quest> ongoingQuests = PrototypeManager.Quest.Values.Where(q => q.IsAccepted && !q.IsCompleted).ToList();

        foreach (Quest quest in ongoingQuests)
        {
            if (IsQuestCompleted(quest))
            {
                CollectQuestReward(quest);
            }
        }

        List<Quest> completedQuestWithUnCollectedRewards =
            PrototypeManager.Quest.Values.Where(q => q.IsCompleted && q.Rewards.Any(r => !r.IsCollected)).ToList();

        foreach (Quest quest in completedQuestWithUnCollectedRewards)
        {
            if (!ongoingQuests.Contains(quest))
            {
                CollectQuestReward(quest);
            }
        }
    }

    private bool IsQuestCompleted(Quest quest)
    {
        quest.IsCompleted = true;
        foreach (QuestGoal goal in quest.Goals)
        {
            FunctionsManager.Quest.Call(goal.IsCompletedLuaFunction, goal);
            quest.IsCompleted &= goal.IsCompleted;
        }

        return quest.IsCompleted;
    }

    private void CollectQuestReward(Quest quest)
    {
        foreach (QuestReward reward in quest.Rewards)
        {
            if (!reward.IsCollected)
            {
                FunctionsManager.Quest.Call(reward.OnRewardLuaFunction, reward);
            }
        }
    }
}