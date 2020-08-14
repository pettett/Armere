using UnityEngine;
using Yarn.Unity;
using System.Collections;

namespace PlayerController
{

    //Conversation - specific for talking to an NPC
    //Converstation base class - runs dialogue, only requires player controller

    [System.Serializable]
    public class Conversation : MovementState
    {


        public const string GiveQuestCommand = "quest";
        public const string DeliverQuestCommand = "DeliverQuest";
        public const string TalkToQuestCommand = "TalkToQuest";
        public const string CameraPanCommand = "cameraPan";
        public const string GiveItemsCommand = "GiveItems";
        public const string TurnPlayerToTargetCommand = "TurnPlayerToTarget";
        public const string TurnNPCToTargetCommand = "TurnNPCToTarget";
        public const string TurnNPCAndPlayerToTargetCommand = "TurnNPCAndPlayerToTarget";
        public const string AnimateCommand = "Animate";
        public const string OfferToSellCommand = "OfferToSell";
        public const string OfferToBuyCommand = "OfferToBuy";
        public const string GoToCommand = "GoTo";

        public override string StateName => "In Conversation";
        DialogueRunner runner => c.runner;


        public NPC talkingTarget;

        BuyInventoryUI buyMenu;
        public override void Start(params object[] args)
        {

            if (args[0] is NPC npc)
            {
                talkingTarget = npc;
            }
            else
            {
                throw new System.Exception("First arg must be npc");
            }

            //Setup PlayerController for static conversation
            c.cameraController.lockingMouse = false;
            c.rb.velocity = Vector3.zero;
            c.cameraController.DisableControl();
            c.rb.isKinematic = true;
            GameCameras.s.cutsceneCamera.Priority = 50;

            //Set up ik weights
            c.animationController.headLookAtPositionWeight = 1;
            c.animationController.lookAtPositionWeight = 1;
            c.animationController.clampLookAtPositionWeight = 0.5f; //Half rotation away


            c.animationController.lookAtPosition = talkingTarget.headPosition.position;

            SetupRunner();
        }
        public override void End()
        {
            CleanUpRunner();
            c.cameraController.lockingMouse = true;
            c.rb.isKinematic = false;
            c.cameraController.EnableControl();
            GameCameras.s.cutsceneCamera.Priority = 0;

            c.animationController.headLookAtPositionWeight = 0;
            c.animationController.lookAtPositionWeight = 0;
        }

        public override void Update()
        {



        }

        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.vertical.id, 0);
        }


        public void RunSellMenu(System.Action<ItemType, int> onSelectItem)
        {
            UIController.singleton.sellMenu.SetActive(true);
            //Wait for a buy
            UIController.singleton.sellMenu.GetComponentInChildren<InventoryUI>().onItemSelected = onSelectItem;
        }
        public void CloseSellMenu()
        {
            //Close the buy menu
            UIController.singleton.sellMenu.SetActive(false);
            UIController.singleton.sellMenu.GetComponentInChildren<InventoryUI>().onItemSelected = null;
        }

        public override void OnAnimatorIK(int layerIndex)
        {
            //Make head look at npc
        }

        void SetDialogBoxActive(bool active) => DialogueInstances.singleton.dialogueUI.dialogueContainer.SetActive(active);
        void EnableDialogBox() => SetDialogBoxActive(true);
        void DisableDialogBox() => SetDialogBoxActive(false);

        public void SetupRunner()
        {
            //Add every command hander for dialogue usage
            runner.Add(talkingTarget.t.dialogue);

            DialogueUI.singleton.onLineStart.AddListener(talkingTarget.StartNPCSpeaking);
            DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);

            runner.StartDialogue(talkingTarget.ConversationStartNode);

            runner.AddCommandHandler(GiveQuestCommand, GiveQuest);
            runner.AddCommandHandler(DeliverQuestCommand, DeliverQuest);
            runner.AddCommandHandler(TalkToQuestCommand, TalkToQuest);
            runner.AddCommandHandler(CameraPanCommand, CameraPan);
            runner.AddCommandHandler(GiveItemsCommand, GiveItems);
            runner.AddCommandHandler(TurnPlayerToTargetCommand, TurnPlayerToTarget);
            runner.AddCommandHandler(TurnNPCToTargetCommand, TurnNPCToTarget);
            runner.AddCommandHandler(TurnNPCAndPlayerToTargetCommand, TurnNPCAndPlayerToTarget);
            runner.AddCommandHandler(AnimateCommand, Animate);
            runner.AddCommandHandler(OfferToSellCommand, OfferToSell);
            runner.AddCommandHandler(OfferToBuyCommand, OfferToBuy);
            runner.AddCommandHandler(GoToCommand, GoTo);

            (runner.variableStorage as InMemoryVariableStorage).addon = talkingTarget;
        }

        public void CleanUpRunner()
        {
            print("Cleaning up dialogue runner after dialogue");
            //Remove all commands from the runner as well as removing dialogue
            runner.Clear();
            runner.ClearStringTable();
            runner.Stop();
            runner.RemoveCommandHandler(GiveQuestCommand);
            runner.RemoveCommandHandler(DeliverQuestCommand);
            runner.RemoveCommandHandler(TalkToQuestCommand);
            runner.RemoveCommandHandler(CameraPanCommand);
            runner.RemoveCommandHandler(GiveItemsCommand);
            runner.RemoveCommandHandler(TurnPlayerToTargetCommand);
            runner.RemoveCommandHandler(TurnNPCToTargetCommand);
            runner.RemoveCommandHandler(TurnNPCAndPlayerToTargetCommand);
            runner.RemoveCommandHandler(AnimateCommand);
            runner.RemoveCommandHandler(OfferToSellCommand);
            runner.RemoveCommandHandler(OfferToBuyCommand);
            runner.RemoveCommandHandler(GoToCommand);

            DialogueUI.singleton.onDialogueEnd.RemoveListener(OnDialogueComplete);
            DialogueUI.singleton.onLineStart.RemoveListener(talkingTarget.StartNPCSpeaking);

            (runner.variableStorage as InMemoryVariableStorage).addon = null;
        }
        #region Yarn Commands
        void Animate(string[] arg, System.Action onComplete)
        {
            string animName = arg[0];
            if (arg.Length > 1)
            {
                bool wait = bool.Parse(arg[1]);
            }
            animator.SetTrigger(animName);
            onComplete?.Invoke();
        }

        void GiveQuest(string[] arg)
        {
            string questName = arg[0];
            for (int i = 0; i < talkingTarget.t.quests.Length; i++)
            {
                if (talkingTarget.t.quests[i].name == questName)
                {
                    QuestManager.AddQuest(talkingTarget.t.quests[i]);
                }
            }
        }
        void DeliverQuest(string[] arg) => QuestManager.ForfillDeliverQuest(arg[0]);

        void TalkToQuest(string[] arg) => QuestManager.ForfillTalkToQuest(arg[0]);


        private void CameraPan(string[] arg, System.Action onComplete)
        {
            Transform focus = talkingTarget.GetFocusPoint(arg[0]);
            if (focus != null)
                //pan the camera to the target destination
                c.StartCoroutine(TurnCameraToTarget(focus.position, onComplete));
            else
            {
                Debug.LogWarning("Lookat target not in dictionary");
                onComplete?.Invoke();
            }
        }

        IEnumerator TurnCameraToTarget(Vector3 target, System.Action onComplete)
        {
            //Orbit around the focus point
            //
            Quaternion targetRotation = Quaternion.LookRotation((target + Vector3.up));

            //Vector3 pos = focusPoint + Vector3.up + targetRotation * Vector3.back * 2;

            //yield return LerpCameraToPositionAndRotation(pos, targetRotation, 0.3f);

            GameCameras.s.cutsceneCamera.LookAt = c.lookAtTarget;
            c.lookAtTarget.position = target;

            yield return new WaitForSeconds(0.5f);

            onComplete?.Invoke();
        }

        void OnDialogueComplete()
        {
            //Conversation over - clean up
            talkingTarget.FinishSpeaking();

            //playerTransform.ChangeToState<Walking>();

            ResetCamera();

            ChangeToState<Walking>();
        }

        void GoTo(string[] arg, System.Action onComplete)
        {
            DisableDialogBox();
            talkingTarget.GoToWalkingPoint(arg[0],
            () =>
            {
                //Re-enable the dialog box when the AI has finished walking
                EnableDialogBox();
                onComplete?.Invoke();
            }
            );
        }

        public void OfferToBuy(string[] arg)
        {
            buyMenu = UIController.singleton.buyMenu.GetComponent<BuyInventoryUI>();
            print("Opening Buy Menu");
            runner.AddCommandHandler("StopBuy", (a) =>
            {
                runner.RemoveCommandHandler("ConfirmBuy");
                runner.RemoveCommandHandler("CancelBuy");
                runner.RemoveCommandHandler("StopBuy");
                //Apply the changes made to the buy menu to the inventory
                buyMenu.CloseInventory();
            });

            UIController.singleton.buyMenu.SetActive(true);
            //Wait for a buy
            buyMenu.ShowInventory(talkingTarget.buyInventory, InventoryController.singleton.db, OnBuyMenuItemSelected);
        }

        void OnBuyMenuItemSelected()
        {
            void ResetBuyCommands()
            {
                runner.RemoveCommandHandler("ConfirmBuy");
                runner.RemoveCommandHandler("CancelBuy");
            }
            //Buy the item
            runner.AddCommandHandler("ConfirmBuy", (string[] arg) =>
            {
                //Buy the item
                buyMenu.ConfirmBuy();
                ResetBuyCommands();
            });
            //Do not buy the item
            runner.AddCommandHandler("CancelBuy", (string[] arg) =>
            {
                //Go back to selecting
                buyMenu.CancelBuy();
                ResetBuyCommands();
            });

            //update the buy menu with revised amounts

            //Restart the dialog
            DialogueUI.singleton.onDialogueEnd.RemoveListener(OnDialogueComplete);
            runner.Stop();
            DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
            runner.StartDialogue("Buy");
        }

        public void OfferToSell(string[] arg)
        {
            print("Selling");

            runner.AddCommandHandler("StopSell", StopSell);
            //Show the Inventory UI
            RunSellMenu(OnSellMenuItemSelected);
            //Wait for the player to select an item
        }


        void OnSellMenuItemSelected(ItemType type, int itemIndex)
        {

            //Buy the item
            runner.AddCommandHandler("ConfirmSell", (string[] arg) =>
            {
                //Buy the item
                print("Sold Item");

                //TODO - Add amount control
                //Pay the player for the item
                InventoryController.AddItem(ItemName.Currency, InventoryController.singleton.db[InventoryController.ItemAt(itemIndex, type)].sellValue);
                //Remove the item from the inventory
                InventoryController.TakeItem(itemIndex, type);


                ReEnableSellMenu();
            });

            runner.AddCommandHandler("CancelSell", (string[] arg) =>
            {
                //Go back to selecting
                print("Cancelled selection");
                ReEnableSellMenu();
            });

            print("Selected item " + type.ToString());


            //update the buy menu with revised amounts

            //Restart the dialog
            DialogueUI.singleton.onDialogueEnd.RemoveListener(OnDialogueComplete);
            runner.Stop();
            DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
            runner.StartDialogue("Sell");
        }
        void ReEnableSellMenu()
        {
            runner.RemoveCommandHandler("ConfirmSell");
            runner.RemoveCommandHandler("CancelSell");
            // runner.StartDialogue("WaitForSell");
        }

        void StopSell(string[] arg)
        {
            runner.RemoveCommandHandler("ConfirmSell");
            runner.RemoveCommandHandler("CancelSell");
            runner.RemoveCommandHandler("StopSell");
            CloseSellMenu();
        }
        void TurnPlayerToTarget(string[] arg, System.Action onComplete)
        {
            Vector3 target = talkingTarget.GetFocusPoint(arg[0]).position;
            target.y = talkingTarget.transform.position.y;
            talkingTarget.transform.LookAt(target);
            onComplete?.Invoke();
        }
        void TurnNPCToTarget(string[] arg, System.Action onComplete)
        {
            Vector3 target = talkingTarget.GetFocusPoint(arg[0]).position;
            target.y = talkingTarget.transform.position.y;
            talkingTarget.transform.LookAt(target);
            onComplete?.Invoke();
        }
        void TurnNPCAndPlayerToTarget(string[] arg, System.Action onComplete)
        {
            Transform focus = talkingTarget.GetFocusPoint(arg[0]);
            if (focus != null)
            {
                Vector3 target = focus.position;
                target.y = talkingTarget.transform.position.y;
                talkingTarget.transform.LookAt(target);
                target.y = transform.position.y;
                transform.LookAt(target);

                c.animationController.lookAtPosition = focus.position;


            }
            else
            {
                Debug.LogWarning("Lookat target not in dictionary");
            }
            onComplete?.Invoke();
        }

        void GiveItems(string[] arg, System.Action onComplete)
        {
            ItemName item = (ItemName)System.Enum.Parse(typeof(ItemName), arg[0]);
            uint count;
            if (arg.Length == 1)
                count = 1;
            else
                count = uint.Parse(arg[1]);

            //give [count] items of type [item]
            NewItemPrompt.singleton.ShowPrompt(item, count, onComplete);
        }



        void ResetCamera()
        {
            GameCameras.s.cutsceneCamera.LookAt = GameCameras.s.conversationGroup.Transform;
        }


        #endregion
    }
}