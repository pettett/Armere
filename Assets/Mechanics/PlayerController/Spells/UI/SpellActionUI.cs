using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Armere.PlayerController.UI
{
	public class SpellActionUI : MonoBehaviour
	{
		[System.NonSerialized] public SpellAction action;
		public Image thumbnail;
		public void SetAction(SpellAction action)
		{
			this.action = action;
			if (action != null)
				thumbnail.sprite = action.sprite;
		}
	}
}