using System.Collections.Generic;
using System.Xml;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class Quest
{
    public string Name;
    public string Description;

    public List<QuestGoal> Goals;

    public bool IsAccepted;
    public bool IsCompleted;

    public List<QuestReward> Rewards;

    public void ReadXmlPrototype(XmlTextReader reader_parent)
    {
        Name = reader_parent.GetAttribute("Name");

        XmlReader reader = reader_parent.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Description":
                    reader.Read();
                    Description = reader.ReadContentAsString();
                    break;
                case "Goals":
                    Goals = new List<QuestGoal>();
                    XmlReader goals_reader = reader.ReadSubtree();
                    while (goals_reader.Read())
                    {
                        if (goals_reader.Name == "Goal")
                        {
                            QuestGoal goal = new QuestGoal();
                            goal.ReadXmlPrototype(goals_reader);
                            Goals.Add(goal);
                        }
                    }
                    break;
                case "Rewards":
                    Rewards = new List<QuestReward>();    
                    XmlReader reward_reader = reader.ReadSubtree();
                    while (reward_reader.Read())
                    {
                        if (reward_reader.Name == "Reward")
                        {
                            QuestReward reward = new QuestReward();
                            reward.ReadXmlPrototype(reward_reader);
                            Rewards.Add(reward);
                        }
                    }
                    break;
            }
        }
    }
}