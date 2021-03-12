using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Conversation")]
	public class ConversationTemplate : DialogueTemplate
	{
		[System.NonSerialized] public AIDialogue npc;
		public override MovementState StartState(PlayerController c)
		{
			return new Conversation(c, this);
		}

		public ConversationTemplate Interact(AIDialogue c)
		{

			dialogue = npc = c;
			return this;
		}
	}
}