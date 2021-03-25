using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Armere.Inventory.UI
{
	public class InventoryItemUI : Selectable
	{
		//Set by script
		public int itemIndex;
		public ItemType type;
		//Set by hand
		public Image thumbnail;
		public TMPro.TextMeshProUGUI countText;
		public TMPro.TextMeshProUGUI infoText;
		public TMPro.TextMeshProUGUI nameText;
		AsyncOperationHandle<Sprite> asyncOperation;

		public IntEventChannelSO changeItemIndexEventChannel;
		protected override void Start()
		{
			if (changeItemIndexEventChannel != null)
				changeItemIndexEventChannel.OnEventRaised += ChangeItemIndex;

			base.Start();
		}
		public void ChangeItemIndex(int newIndex)
		{

			itemIndex = newIndex;
			if (itemIndex != -1)
				SetupItemDisplayAsync(InventoryController.singleton.ItemAt(itemIndex, type));
		}

		public void SetupItemDisplayAsync(ItemStackBase item)
		{
			ReleaseCurrentSprite();

			ItemData data = item.item;
			type = data.type;

			switch (data)
			{
				case MeleeWeaponItemData melee:
					infoText?.SetText(melee.damage.ToString());
					break;
				default:
					if (infoText != null)
						Destroy(infoText.transform.parent.gameObject);

					break;
			}
			uint count = item switch
			{
				ItemStack s => s.count,
				_ => 0
			};

			SetupItemDisplayAsync(item.item, count);
		}




		public void SetupItemDisplayAsync(ItemData displayItem, uint count)
		{
			if (countText != null)
				if (count == 0)
				{
					countText.enabled = false;
				}
				else
				{
					countText.SetText(count.ToString());
				}
			if (thumbnail != null)
				Spawner.LoadAsset(displayItem.thumbnail, (handle) =>
				{
					asyncOperation = handle;
					//The image may have been destroyed before finishing
					thumbnail.sprite = handle.Result;
					thumbnail.color = Color.white;
				});

			nameText?.SetText(displayItem.displayName);
		}


		new private void OnDestroy()
		{
			ReleaseCurrentSprite();
		}
		public void ReleaseCurrentSprite()
		{
			Spawner.ReleaseAsset(asyncOperation);
			thumbnail.color = Color.clear;
		}

	}
}