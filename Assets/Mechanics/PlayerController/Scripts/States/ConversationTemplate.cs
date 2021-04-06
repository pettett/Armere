using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Conversation")]
	public class ConversationTemplate : DialogueTemplate
	{
		public NPCManager npcManager;
		[System.NonSerialized] public AIDialogue npc;
		[System.NonSerialized] public string overrideStartingNode;
		public override MovementState StartState(PlayerController c)
		{
			return new Conversation(c, this, npc);
		}

		public ConversationTemplate StartConversation(AIDialogue target, string overrideStartingNode = null)
		{
			dialogue = npc = target;
			this.overrideStartingNode = overrideStartingNode;
			return this;
		}

		public void StartConversation(PlayerController controller, AIDialogue target, string overrideStartingNode = null)
		{
			controller.ChangeToState(StartConversation(target, overrideStartingNode));
		}

		public void TeleportToConversation(PlayerController controller, AIDialogue target, string overrideStartingNode = null)
		{
			controller.Warp(target.transform.position + target.transform.forward * 1.5f);
			controller.ChangeToState(StartConversation(target, overrideStartingNode));
		}
	}
}