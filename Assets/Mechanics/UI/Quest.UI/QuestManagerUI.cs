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
	public QuestManager questManager;
	private void OnEnable()
	{
		if (questManager != null)
			for (int i = 0; i < questManager.quests.Count; i++)
			{
				var b = Instantiate(questButtonPrefab, questButtonContent);
				int index = i;
				b.GetComponent<Button>().onClick.AddListener(() => SelectQuest(index, false));
				b.GetComponentInChildren<TextMeshProUGUI>().text = questManager.quests[i].quest.title;
			}
	}

	private void OnDisable()
	{
		for (int i = 0; i < questButtonContent.childCount; i++)
		{
			Destroy(questButtonContent.GetChild(i).gameObject);
		}
	}

	void SelectQuest(int i, bool completed)
	{
		questTitle.text = questManager.quests[i].quest.title;
		Quest.QuestStage stage = questManager.quests[i].quest.stages[questManager.quests[i].stage];
		questDescription.text = stage.description;
		questManager.selectedQuest = questManager.quests[i];

		questGoal.text = string.Format("Deliver <color=#{0}>{1}</color> <color=#{2}>{3}</color> to <color=#{4}>{5}</color>",
			styling.numberColorHex, stage.count,
			styling.itemNameColorHex, stage.item.ToString(),
			styling.NPCNameColorHex, stage.receiver.ToString());
	}
}
