using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestController
{
    private float totalDeltaTime;
    private readonly float checkDelayInSeconds;

    public QuestController()
    {
        checkDelayInSeconds = 5f;
        LoadLuaScript();
    }

    void LoadLuaScript()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA");
        filePath = System.IO.Path.Combine(filePath, "Quest.lua");
        string luaCode = System.IO.File.ReadAllText(filePath);

        new QuestActions(luaCode);
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
        List<Quest> ongoingQuests = World.current.Quests.Where(q => q.IsAccepted && !q.IsCompleted).ToList();

        foreach (Quest quest in ongoingQuests)
        {
            if (IsQuestCompleted(quest))
            {
                CollectQuestReward(quest);
            }
        }

        List<Quest> completedQuestWithUnCollectedRewards =
            World.current.Quests.Where(q => q.IsCompleted && q.Rewards.Any(r => !r.IsCollected)).ToList();

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
            QuestActions.CallFunction(goal.IsCompletedLuaFunction, goal);
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
                QuestActions.CallFunction(reward.OnRewardLuaFunction, reward);
            }
        }
    }
}