using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.UI;

namespace Armere.PlayerController.UI
{

	public class InteractablePrompts : MonoBehaviour
	{
		public UIPrompt prompt;
		public WorldIndicator indicator;

		public InteractTemplate interact;
		public InputReader inputReader;
		private void OnEnable()
		{
			interact.OnBeginHighlight += BeginHighlight;
			interact.OnEndHighlight += EndHighlight;
		}
		private void OnDisable()
		{
			interact.OnBeginHighlight -= BeginHighlight;
			interact.OnEndHighlight -= EndHighlight;
		}

		private void BeginHighlight(IInteractable interactable)
		{
			prompt.ApplyPrompt(interactable.interactionDescription, inputReader.GetActionDisplayName("Action"));
			if (interactable.interactionName != null)
				indicator.StartIndication(interactable.gameObject.transform, interactable.interactionName);
		}

		private void EndHighlight()
		{
			prompt.ResetPrompt();
			indicator.EndIndication();
		}
	}
}