using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Interact")]
	public class InteractTemplate : MovementStateTemplate
	{
		public ConversationTemplate interactNPC;
		public LadderClimbTemplate interactLadder;
		public DialogueTemplate interactDialogue;


		public System.Action<IInteractable> OnBeginHighlight;
		public System.Action OnEndHighlight;

		public override MovementState StartState(PlayerController c)
		{
			return new Interact(c, this);
		}

	}
}