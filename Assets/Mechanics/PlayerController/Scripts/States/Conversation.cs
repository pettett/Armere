using UnityEngine;
using Yarn.Unity;
using System.Collections;
using Cinemachine;
using Armere.Inventory.UI;
using Armere.Inventory;
using Yarn;

namespace Armere.PlayerController
{

	//Conversation - specific for talking to an NPC
	//Converstation base class - runs dialogue, only requires player controller


	public class Conversation : Dialogue<ConversationTemplate>
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
		public readonly AIDialogue talkingTarget;


		public readonly (string, DialogueRunner.CommandHandler)[] commands;
		public readonly (string, DialogueRunner.BlockingCommandHandler)[] blockingCommands;


		BuyInventoryUI buyMenu;
		bool hasSpeaker;
		NPCName speakingNPC;

		public Conversation(PlayerController c, ConversationTemplate t) : base(c, t)
		{

			commands = new (string, DialogueRunner.CommandHandler)[]
			{
				(GiveQuestCommand, GiveQuest),
				(DeliverQuestCommand, DeliverQuest),
				(TalkToQuestCommand, TalkToQuest),



				(OfferToSellCommand, OfferToSell),
				(OfferToBuyCommand, OfferToBuy)

			};
			blockingCommands = new (string, DialogueRunner.BlockingCommandHandler)[]{
			(CameraPanCommand, CameraPan),
						(GiveItemsCommand, GiveItems),
									(TurnPlayerToTargetCommand, TurnPlayerToTarget),

				(TurnNPCToTargetCommand, TurnNPCToTarget),
				(TurnNPCAndPlayerToTargetCommand, TurnNPCAndPlayerToTarget),
				(AnimateCommand, Animate),
							(GoToCommand, GoTo)
			};

			talkingTarget = t.npc;


			//Set up ik weights
			c.animationController.headLookAtPositionWeight = 1;
			c.animationController.lookAtPositionWeight = 1;
			c.animationController.clampLookAtPositionWeight = 0.5f; //Half rotation away
			c.animationController.lookAtPosition = talkingTarget.headPosition.position;

			//GameCameras.s.freeLookTarget = GameCameras.s.cameraTrackingTarget;

			//Setup camera
			int targets = Mathf.Max(2, talkingTarget.conversationGroupOverride.Length + 1);
			//Add all targets including the player
			Transform[] targetTransforms = new Transform[targets];
			if (talkingTarget.conversationGroupOverride.Length != 0)
			{
				for (int i = 0; i < talkingTarget.conversationGroupOverride.Length; i++)
				{
					targetTransforms[i + 1] = talkingTarget.conversationGroupOverride[i];
				}
			}
			else
			{
				targetTransforms[1] = talkingTarget.transform;
			}

			GameCameras.s.SetCameraTargets(targetTransforms);
			GameCameras.s.conversationGroup.DoUpdate();

			//Add the variables for this NPC

			GameCameras.s.conversationCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = false;


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
			foreach (var pair in commands)
			{
				runner.AddCommandHandler(pair.Item1, pair.Item2);
			}
			foreach (var pair in blockingCommands)
			{
				runner.AddCommandHandler(pair.Item1, pair.Item2);
			}
			(runner.variableStorage as InMemoryVariableStorage).addons.Add(talkingTarget.dialogueAddon);
		}

		public override void CleanUpRunner()
		{
			Debug.Log("Cleaning up dialogue runner after dialogue");
			//Remove all commands from the runner as well as removing dialogue
			base.CleanUpRunner();

			foreach (var pair in commands)
			{
				runner.RemoveCommandHandler(pair.Item1);
			}
			foreach (var pair in blockingCommands)
			{
				runner.RemoveCommandHandler(pair.Item1);
			}

			(runner.variableStorage as InMemoryVariableStorage).addons.Remove(talkingTarget.dialogueAddon);
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
				Debug.Log(line);
				throw ex;
			}

			if (!hasSpeaker)
			{
				GameCameras.s.conversationCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = GetCameraAngle(NPC.activeNPCs[currentSpeaker].transform);
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
			QuestManager.singleton.AddQuest(questName);
		}

		void DeliverQuest(string[] arg) => QuestManager.singleton.ForfillDeliverQuest(arg[0]);

		void TalkToQuest(string[] arg) => QuestManager.singleton.ForfillTalkToQuest(arg[0], talkingTarget.npcName);


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
			GameCameras.s.conversationCamera.LookAt = target;

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
			Debug.Log("Opening Buy Menu");
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
			Debug.Log("Selling");
			//Show the Inventory UI
			//Wait for the player to select an item      
			RunSellMenu();
		}
		public async void RunSellMenu()
		{

			DialogueUI.singleton.onDialogueEnd.RemoveListener(OnDialogueComplete);
			(ItemType type, int index) = await ItemSelectionMenuUI.singleton.SelectItem(x => x.item.sellable);

			if (index != -1)
			{
				DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
				OnSellMenuItemSelected(type, index);
			}//else the dialogue will handle it
			else if (!DialogueRunner.singleton.IsDialogueRunning)
			{
				OnDialogueComplete();
			}
			else
			{
				DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
			}

		}

		void OnSellMenuItemSelected(ItemType type, int itemIndex)
		{
			//Pay the player for the item
			c.inventory.currency.AddItem(c.inventory.ItemAt(itemIndex, type).item, 1);

			//Remove the item from the inventory
			c.inventory.TakeItem(itemIndex, type);

			RunSellMenu();
		}

		void HideAllDialogueButtons()
		{
			foreach (var button in DialogueUI.singleton.optionButtons)
			{
				button.gameObject.SetActive(false);
			}
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
			NewItemPrompt.singleton.ShowPrompt(InventoryController.singleton.db[item], count, onComplete);
		}



		void ResetCamera()
		{
			GameCameras.s.conversationCamera.LookAt = GameCameras.s.conversationGroup.Transform;
		}


		#endregion
	}
}