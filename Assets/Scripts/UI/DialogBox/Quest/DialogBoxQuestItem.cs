using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxQuestItem : MonoBehaviour
{
    public Quest Quest;
    private DialogBoxQuests parentDialogBoxQuests;
    public Text TitleText;
    public Text DescriptionText;
    public Text GoalText;
    public Text RewardText;
    public Button AcceptButton;

    public void SetupQuest(DialogBoxQuests parent, Quest item)
    {
        parentDialogBoxQuests = parent;
        SetupQuest(item);
    }

    public void SetupQuest( Quest item)
    {
        Quest = item;
        BindInterface();
    }

    private void BindInterface()
    {
        if (Quest == null)
            return;

        TitleText.text = Quest.Name;
        DescriptionText.text = Quest.Description;
        GoalText.text = "Goals: " +
                        Environment.NewLine +
                        string.Join(Environment.NewLine,
                            (string[]) Quest.Goals.Select(g => GetGoalText(g)).ToArray<string>());
        RewardText.text = "Rewards: " +
                        Environment.NewLine +
                        string.Join(Environment.NewLine,
                            Quest.Rewards.Select(r => "   - " + r.Description).ToArray());
        AcceptButton.gameObject.SetActive(!Quest.IsAccepted);
    }

    private string GetGoalText(QuestGoal questGoal)
    {
        if (questGoal.IsCompleted)
        {
            return "<color=green>   - " + questGoal.Description + "</color>";
        }
        return "<color=red>   - " + questGoal.Description + "</color>";
    }

    public void Update()
    {
        if (parentDialogBoxQuests == null)
        {
            BindInterface();
        }
    }

    public void OnAccept()
    {
        Quest.IsAccepted = true;

        if (parentDialogBoxQuests != null)
        {
            parentDialogBoxQuests.CloseDialog();
        }
    }
}