using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Armere.PlayerController.UI
{
	[RequireComponent(typeof(SpellActionUI))]
	public class SpellStatusUI : MonoBehaviour
	{
		SpellStatus status;
		SpellActionUI actionUI;
		private void Awake()
		{
			actionUI = GetComponent<SpellActionUI>();
		}
		public void SetStatus(SpellStatus status)
		{
			this.status = status;
			OnStatusChanged();
			status.onSpellChanged += OnStatusChanged;
		}
		public void OnStatusChanged()
		{

			if (status.spell != null)
				actionUI.SetAction(status.spell);
		}
	}
}