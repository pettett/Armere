using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace Armere.Inventory.UI
{
    public class ItemSelectionMenuUI : MonoBehaviour
    {
        public static ItemSelectionMenuUI singleton;

        public InventoryUI inventoryUI;


        private void Awake()
        {
            singleton = this;
        }

        public async Task<(int, ItemType)> SelectItem()
        {
            await Task.Delay(500);

            return (0, ItemType.Common);
        }

        private async void Start()
        {
            print(await SelectItem());
        }

    }
}