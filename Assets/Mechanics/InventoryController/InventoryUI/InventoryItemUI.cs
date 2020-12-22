using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Armere.Inventory.UI
{
    public class InventoryItemUI : MonoBehaviour
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
        public void ChangeItemIndex(int newIndex)
        {

            itemIndex = newIndex;
            if (itemIndex != -1)
                SetupItemAsync(InventoryController.ItemAt(itemIndex, type));
        }

        public async void SetupItemAsync(ItemStackBase item)
        {
            ItemData data = InventoryController.singleton.db[item.name];
            type = data.type;

            switch (data)
            {
                case MeleeWeaponItemData melee:
                    infoText?.SetText(melee.damage.ToString());
                    if (countText != null) countText.enabled = false;

                    break;
                default:
                    if (infoText != null)
                        Destroy(infoText.transform.parent.gameObject);

                    break;
            }

            if (data.displaySprite.RuntimeKeyIsValid())
            {
                asyncOperation = Addressables.LoadAssetAsync<Sprite>(data.displaySprite);
                Sprite s = await asyncOperation.Task;
                //The image may have been destroyed before finishing
                if (thumbnail != null) thumbnail.sprite = s;
            }
            nameText?.SetText(data.displayName);




        }


        private void OnDestroy()
        {
            ReleaseCurrentSprite();
        }
        public void ReleaseCurrentSprite()
        {
            if (asyncOperation.IsValid())
                Addressables.Release(asyncOperation);
        }

    }
}