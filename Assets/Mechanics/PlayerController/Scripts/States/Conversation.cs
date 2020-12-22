using UnityEngine;
using Yarn.Unity;
using System.Collections;
using Cinemachine;
using Armere.Inventory.UI;
using Armere.Inventory;

namespace Armere.PlayerController
{

    //Conversation - specific for talking to an NPC
    //Converstation base class - runs dialogue, only requires player controller

    [System.Serializable]
    public class Conversation : Dialogue
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
        public NPC talkingTarget;

        BuyInventoryUI buyMenu;
        bool hasSpeaker;
        NPCName speakingNPC;

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

            //Set up ik weights
            c.animationController.headLookAtPositionWeight = 1;
            c.animationController.lookAtPositionWeight = 1;
            c.animationController.clampLookAtPositionWeight = 0.5f; //Half rotation away
            c.animationController.lookAtPosition = talkingTarget.headPosition.position;

            //Setup camera
            int targets = Mathf.Max(2, talkingTarget.conversationGroupOverride.Length + 1);
            //Add all targets including the player
            GameCameras.s.conversationGroup.m_Targets = new CinemachineTargetGroup.Target[targets];
            GameCameras.s.conversationGroup.m_Targets[0] = GenerateTarget(transform);

            if (talkingTarget.conversationGroupOverride.Length != 0)
            {
                for (int i = 0; i < talkingTarget.conversationGroupOverride.Length; i++)
                {
                    GameCameras.s.conversationGroup.m_Targets[i + 1] = GenerateTarget(talkingTarget.conversationGroupOverride[i]);
                }
            }
            else
            {
                GameCameras.s.conversationGroup.m_Targets[1] = GenerateTarget(talkingTarget.transform);
            }

            GameCameras.s.conversationGroup.DoUpdate();

            //Add the variables for this NPC

            GameCameras.s.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = false;


            StartDialogue(talkingTarget);
        }



        public override void End()
        {
            base.End();


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

        public override void SetupRunner()
        {
            //Add every command hander for dialogue usage

            base.SetupRunner();

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

            (runner.variableStorage as InMemoryVariableStorage).addons.Add(talkingTarget);
        }

        public override void CleanUpRunner()
        {
            print("Cleaning up dialogue runner after dialogue");
            //Remove all commands from the runner as well as removing dialogue
            base.CleanUpRunner();

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


            (runner.variableStorage as InMemoryVariableStorage).addons.Remove(talkingTarget);
        }



        public override void OnLineStart(string line)
        {

            if (line == null) return;

            NPCName currentSpeaker;
            try
            {
                currentSpeaker = (NPCName)System.Enum.Parse(typeof(NPCName), line.Split(':')[0]);
            }
            catch (System.Exception ex)
            {
                print(line);
                throw ex;
            }

            if (!hasSpeaker)
            {
                GameCameras.s.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = GetCameraAngle(NPC.activeNPCs[currentSpeaker].transform);
                NPC.activeNPCs[currentSpeaker].StartSpeaking(transform);
                hasSpeaker = true;
            }
            else if (speakingNPC != currentSpeaker)
            {
                NPC.activeNPCs[speakingNPC].StopSpeaking();
                NPC.activeNPCs[currentSpeaker].StartSpeaking(transform);
                //Re-target camera to point to the new speaker
                PointCameraToSpeaker(NPC.activeNPCs[currentSpeaker].transform);
            }
            speakingNPC = currentSpeaker;

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
            QuestManager.AddQuest(questName);
        }

        void DeliverQuest(string[] arg) => QuestManager.ForfillDeliverQuest(arg[0]);

        void TalkToQuest(string[] arg) => QuestManager.ForfillTalkToQuest(arg[0], talkingTarget.npcName);


        private void CameraPan(string[] arg, System.Action onComplete)
        {
            Transform focus = talkingTarget.GetFocusPoint(arg[0]);
            if (focus != null)
                //pan the camera to the target destination
                c.StartCoroutine(TurnCameraToTarget(focus, onComplete));
            else
            {
                Debug.LogWarning("Lookat target not in dictionary");
                onComplete?.Invoke();
            }
        }

        IEnumerator TurnCameraToTarget(Transform target, System.Action onComplete)
        {
            GameCameras.s.cutsceneCamera.LookAt = target;

            yield return new WaitForSeconds(0.5f);

            onComplete?.Invoke();
        }



        public override void OnDialogueComplete()
        {
            //Conversation over - clean up
            NPC.activeNPCs[speakingNPC].StopSpeaking();

            //playerTransform.ChangeToState<Walking>();

            ResetCamera();

            base.OnDialogueComplete();
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
            HideAllDialogueButtons();
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
                InventoryController.singleton.currency.TakeItem(InventoryController.ItemAt(itemIndex, type).name, 1);
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
            HideAllDialogueButtons();
            DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
            runner.StartDialogue("Sell");
        }

        void HideAllDialogueButtons()
        {
            foreach (var button in DialogueUI.singleton.optionButtons)
            {
                button.gameObject.SetActive(false);
            }
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