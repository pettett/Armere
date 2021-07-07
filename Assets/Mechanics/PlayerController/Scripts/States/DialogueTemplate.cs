using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Dialogue")]
	public class DialogueTemplate : MovementStateTemplate
	{
		[System.NonSerialized] public IDialogue dialogue;
		public override MovementState StartState(PlayerMachine c)
		{
			return new Dialogue<DialogueTemplate>(c, this);
		}

		public DialogueTemplate Interact(IDialogue d)
		{
			dialogue = d;
			return this;
		}
	}
}