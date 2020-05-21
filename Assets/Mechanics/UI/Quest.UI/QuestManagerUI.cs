using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class QuestManagerUI : MonoBehaviour
{
    public GameObject questButtonPrefab;
    public RectTransform questButtonContent;
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;
    public TextMeshProUGUI questGoal;
    public UIStyle styling;
    private void OnEnable()
    {
        if (QuestManager.singleton != null)
            for (int i = 0; i < QuestManager.singleton.quests.Count; i++)
            {
                var b = Instantiate(questButtonPrefab, questButtonContent);
                int index = i;
                b.GetComponent<Button>().onClick.AddListener(() => ViewQuest(index, false));
                b.GetComponentInChildren<TextMeshProUGUI>().text = QuestManager.singleton.quests[i].quest.title;
            }
    }

    private void OnDisable()
    {
        for (int i = 0; i < questButtonContent.childCount; i++)
        {
            Destroy(questButtonContent.GetChild(i).gameObject);
        }
    }

    void ViewQuest(int i, bool completed)
    {
        questTitle.text = QuestManager.singleton.quests[i].quest.title;
        Quest.QuestStage stage = QuestManager.singleton.quests[i].quest.stages[QuestManager.singleton.quests[i].stage];
        questDescription.text = stage.description;

        questGoal.text = string.Format("Deliver <color=#{0}>{1}</color> <color=#{2}>{3}</color> to <color=#{4}>{5}</color>",
            styling.numberColorHex, stage.count,
            styling.itemNameColorHex, stage.item.ToString(),
            styling.NPCNameColorHex, stage.receiver.ToString());
    }
}
