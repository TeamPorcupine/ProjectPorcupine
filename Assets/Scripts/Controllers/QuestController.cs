using System;
using System.Collections.Generic;
using System.Linq;

public class QuestController
{
    private float totalDeltaTime;
    private readonly float checkDelayInSeconds;

    public QuestController()
    {
        checkDelayInSeconds = 5f;
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
    }
    
    private bool IsQuestCompleted(Quest quest)
    {
        quest.IsCompleted = true;
        foreach (QuestGoal goal in quest.Goals)
        {
            FurnitureActions.CallFunction(goal.IsCompletedLuaFunction, goal);
            quest.IsCompleted &= goal.IsCompleted;
        }

        return quest.IsCompleted;
    }

    private void CollectQuestReward(Quest quest)
    {
        foreach (QuestReward reward in quest.Rewards)
        {
            FurnitureActions.CallFunction(reward.OnRewardLuaFunction, reward);
        }
    }
}