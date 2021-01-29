using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

namespace Armere.Inventory.UI
{
	public class TooltipUI : MonoBehaviour
	{
		public static TooltipUI current;

		public TMP_Text title;
		public TMP_Text description;

		private void Awake()
		{
			current = this;
			gameObject.SetActive(false);
		}

		public void OnCursorEnterItemUI(ItemStackBase item)
		{
			title.SetText(item.item.displayName);

			switch (item)
			{
				case PotionItemUnique pot:
					if (pot.item.itemName == ItemName.HealingPotion)
						description.SetText(string.Concat(Enumerable.Repeat('♥', Mathf.RoundToInt(pot.potency))));
					break;
				default:
					description.SetText(new char[0]);
					break;

			}

			gameObject.SetActive(true);
		}
		public void OnCursorExitItemUI()
		{
			gameObject.SetActive(false);
		}

		private void OnGUI()
		{

			transform.position = (Event.current.mousePosition + new Vector2(20, 10)) * new Vector2(1, -1) + new Vector2(0, Screen.height);
		}
	}
}