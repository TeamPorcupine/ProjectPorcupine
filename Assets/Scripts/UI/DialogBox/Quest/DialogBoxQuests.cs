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
        List<Quest> quests = World.current.Quests.Where(q=>!q.IsAccepted).ToList();

        foreach (var quest in quests)
        {
            var go = (GameObject)Instantiate(QuestItemPrefab);
            go.transform.SetParent(QuestItemListPanel);

            var questItemBehaviour = go.GetComponent<DialogBoxQuestItem>();
            questItemBehaviour.SetupQuest(this, quest);
        }
    }
}