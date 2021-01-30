using UnityEngine;
using Yarn;
using Yarn.Unity;
public class QuestStageYarnAddon : IVariableAddon
{
	const string questStagePrefix = "$QuestStage_";

	public string prefix => questStagePrefix;

	public Value this[string name]
	{
		get
		{
			//it is a quest

			if (QuestManager.singleton.TryGetQuest(name, out var q))

				//there is a quest with this name, return it's current state
				return new Value(q.stage);
			else if (QuestManager.singleton.TryGetCompletedQuest(name, out var qw))

				//there is a quest with this name, return it's current state
				return new Value(qw.stage + 1);
			else
				return new Value(-1);
		}
		set => throw new System.NotImplementedException("Cannot set stage of quest");
	}
}