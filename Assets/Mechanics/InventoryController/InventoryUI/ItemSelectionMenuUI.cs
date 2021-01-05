using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Yarn.Unity;

namespace Armere.Inventory.UI
{
    public class ItemSelectionMenuUI : MonoBehaviour
    {
        public static ItemSelectionMenuUI singleton;
        public InventoryUI inventoryUI;
        public RectTransform holder;

        TaskCompletionSource<(ItemType, int)> onItemSelection = null;
        TaskCompletionSource<bool> confirmSelection = null;



        private void Awake()
        {
            singleton = this;
        }

        public async Task<(ItemType type, int index)> SelectItem(System.Predicate<ItemStackBase> predicate)
        {


            DialogueRunner.singleton.AddCommandHandler("ConfirmSelection", ConfirmSelection);
            DialogueRunner.singleton.AddCommandHandler("CancelSelection", CancelSelection);
            DialogueRunner.singleton.AddCommandHandler("StopSelection", StopSelection);
            DialogueRunner.singleton.Stop();
            DialogueInstances.singleton.dialogueUI.CleanUpButtons();

            GameCameras.s.lockingMouse = false;

            holder.gameObject.SetActive(true);
            inventoryUI.EnableMenu(predicate);


            inventoryUI.onItemSelected += OnItemSelected;

            DialogueRunner.singleton.StartDialogue("WaitForSelection");

            bool confirmed = false;
            (ItemType type, int index) result = default;

            while (!confirmed)
            {
                onItemSelection = new TaskCompletionSource<(ItemType, int)>();
                result = await onItemSelection.Task;
                onItemSelection = null;

                if (result.index != -1)
                {
                    DialogueRunner.singleton.Stop();
                    DialogueInstances.singleton.dialogueUI.CleanUpButtons();
                    print("Starting selection");
                    DialogueRunner.singleton.StartDialogue("Select");

                    //Ask if this is what they want
                    confirmSelection = new TaskCompletionSource<bool>();

                    confirmed = await confirmSelection.Task;
                }
                else
                {
                    confirmed = true;
                }
            }
            print("Confirmed, removing commands");

            DialogueRunner.singleton.RemoveCommandHandler("ConfirmSelection");
            DialogueRunner.singleton.RemoveCommandHandler("CancelSelection");
            DialogueRunner.singleton.RemoveCommandHandler("StopSelection");


            holder.gameObject.SetActive(false);
            inventoryUI.DisableMenu();




            return result;
        }


        public void ConfirmSelection(string[] args)
        {
            Debug.Log("Confirmed");
            confirmSelection.SetResult(true);
        }
        public void CancelSelection(string[] args)
        {
            confirmSelection.SetResult(false);
        }

        public void StopSelection(string[] args)
        {
            print("Stopping");
            //If negative one is selected, no confirmation is required
            onItemSelection?.TrySetResult((default, -1));
        }

        void OnItemSelected(ItemType type, int itemIndex)
        {
            onItemSelection?.SetResult((type, itemIndex));
        }
    }
}