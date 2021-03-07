using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Conversation")]
	public class ConversationTemplate : DialogueTemplate
	{
		[System.NonSerialized] public NPC npc;
		public override MovementState StartState(PlayerController c)
		{
			return new Conversation(c, this);
		}

		public ConversationTemplate Interact(NPC c)
		{

			dialogue = npc = c;
			return this;
		}
	}
}