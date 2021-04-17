using UnityEngine;
using Yarn.Unity;
using System.Collections;
using Cinemachine;
using Armere.Inventory.UI;
using Armere.Inventory;
using Yarn;
using Armere.UI;

namespace Armere.PlayerController
{

	//Conversation - specific for talking to an NPC
	//Converstation base class - runs dialogue, only requires player controller


	public class Conversation : Dialogue<ConversationTemplate>
	{
		public const string
			GiveQuestCommand = "quest",
			DeliverQuestCommand = "DeliverQuest",
			TalkToQuestCommand = "TalkToQuest",
			CameraPanCommand = "cameraPan",
			GiveItemsCommand = "GiveItems",
			TurnPlayerToTargetCommand = "TurnPlayerToTarget",
			TurnNPCToTargetCommand = "TurnNPCToTarget",
			TurnNPCAndPlayerToTargetCommand = "TurnNPCAndPlayerToTarget",
			AnimateCommand = "Animate",
			OfferToSellCommand = "OfferToSell",
			OfferToBuyCommand = "OfferToBuy",
			GoToCommand = "GoTo",
			StartMinigameCommand = "StartMinigame",
			ConfirmBuy = "ConfirmBuy",
			CancelBuy = "CancelBuy",
			StopBuy = "StopBuy";

		public override string StateName => "In Conversation";
		public readonly AIDialogue talkingTarget;


		public readonly (string, DialogueRunner.CommandHandler)[] commands;
		public readonly (string, DialogueRunner.BlockingCommandHandler)[] blockingCommands;

		BuyInventoryUI buyMenu;
		string speakingNPC = null;


		public Conversation(PlayerController c, ConversationTemplate t, AIDialogue d) : base(c, t)
		{
			c.StartCoroutine(c.UnEquipAll());
			overrideStartNode = t.overrideStartingNode;

			commands = new (string, DialogueRunner.CommandHandler)[]
			{
				(GiveQuestCommand       , GiveQuest),
				(DeliverQuestCommand    , DeliverQuest),
				(TalkToQuestCommand     , TalkToQuest),
				(OfferToSellCommand     , OfferToSell),
				(OfferToBuyCommand      , OfferToBuy),
				(StartMinigameCommand   , StartMinigame),
			};
			blockingCommands = new (string, DialogueRunner.BlockingCommandHandler)[]{
				(CameraPanCommand,                  CameraPan),
				(GiveItemsCommand,                  GiveItems),
				(TurnPlayerToTargetCommand,         TurnPlayerToTarget),
				(TurnNPCToTargetCommand,            TurnNPCToTarget),
				(TurnNPCAndPlayerToTargetCommand,   TurnNPCAndPlayerToTarget),
				(AnimateCommand,                    Animate),
				(GoToCommand,                       GoTo),
			};

			talkingTarget = t.npc;


			//Set up ik
			c.animationController.SetLookAtTarget(talkingTarget.headPosition);

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
			c.animationController.ClearLookAtTargets();
		}

		public override void Update()
		{
			animator.SetFloat(c.transitionSet.vertical.id, 0);
		}
		public override void OnAnimatorIK(int layerIndex)
		{
			//Make head look at npc
		}

		void SetDialogBoxActive(bool active) => DialogueInstances.singleton.ui.dialogueContainer.SetActive(active);
		void EnableDialogBox() => SetDialogBoxActive(true);
		void DisableDialogBox() => SetDialogBoxActive(false);

		public override void SetupRunner()
		{
			//Add every command hander for dialogue usage

			DialogueInstances.singleton.variableStorage.addons.Add(talkingTarget.dialogueAddon);
			foreach (var pair in commands)
			{
				runner.AddCommandHandler(pair.Item1, pair.Item2);
			}
			foreach (var pair in blockingCommands)
			{
				runner.AddCommandHandler(pair.Item1, pair.Item2);
			}

			base.SetupRunner();
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

			DialogueInstances.singleton.variableStorage.addons.Remove(talkingTarget.dialogueAddon);
		}



		public override void OnLineStart(string line)
		{

			if (line == null) return;

			string currentSpeaker;
			try
			{
				currentSpeaker = line.Split(':')[0];
			}
			catch (System.Exception ex)
			{
				Debug.Log(line);
				throw ex;
			}

			if (speakingNPC == null)
			{
				GameCameras.s.conversationCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = GetCameraAngle(NPCRoutine.activeNPCs[currentSpeaker].transform);
				NPCRoutine.activeNPCs[currentSpeaker].StartSpeaking(transform);
			}
			else if (speakingNPC != currentSpeaker)
			{
				NPCRoutine.activeNPCs[speakingNPC].StopSpeaking();
				NPCRoutine.activeNPCs[currentSpeaker].StartSpeaking(transform);
				//Re-target camera to point to the new speaker
				PointCameraToSpeaker(NPCRoutine.activeNPCs[currentSpeaker].transform);
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
			NPCRoutine.activeNPCs[speakingNPC].StopSpeaking();

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
			//Debug.Log("Opening Buy Menu");
			runner.AddCommandHandler(StopBuy, (a) =>
			{
				runner.RemoveCommandHandler(ConfirmBuy);
				runner.RemoveCommandHandler(CancelBuy);
				runner.RemoveCommandHandler(StopBuy);
				//Apply the changes made to the buy menu to the inventory
				buyMenu.CloseInventory();
			});

			buyMenu.gameObject.SetActive(true);
			//Wait for a buy
			buyMenu.ShowInventory(talkingTarget.buyInventory, c.inventory, OnBuyMenuItemSelected);
		}
		public void StartMinigame(string[] args)
		{

			var minigame = talkingTarget.minigames[int.Parse(args[0])];
			string finishedConversationNode = args[1];

			minigame.StartMinigame(c);

			void OnMinigameEnd(object result)
			{
				//Apply the value
				//t.npcManager.data[talkingTarget.npcName].AddVariable(minigame.minigameResultVariableName, new Yarn.Value(result));
				//Use the value
				t.TeleportToConversation(c, talkingTarget, finishedConversationNode);

				//Forget the value
				minigame.onMinigameEnded -= OnMinigameEnd;
			}

			minigame.onMinigameEnded += OnMinigameEnd;


			DialogueInstances.singleton.variableStorage.addons.Add(minigame);

			//TODO: End conversation
			//c.ChangeToState(c.defaultState);
		}


		public void EndMinigame(string[] args)
		{

			var minigame = talkingTarget.minigames[int.Parse(args[0])];
			DialogueInstances.singleton.variableStorage.addons.Remove(minigame);
		}

		void OnBuyMenuItemSelected()
		{
			void ResetBuyCommands()
			{
				runner.RemoveCommandHandler(ConfirmBuy);
				runner.RemoveCommandHandler(CancelBuy);
			}
			//Buy the item
			runner.AddCommandHandler(ConfirmBuy, (string[] arg) =>
			{
				//Buy the item
				buyMenu.ConfirmBuy();
				ResetBuyCommands();
			});
			//Do not buy the item
			runner.AddCommandHandler(CancelBuy, (string[] arg) =>
			{
				//Go back to selecting
				buyMenu.CancelBuy();
				ResetBuyCommands();
			});

			//update the buy menu with revised amounts

			//Restart the dialog
			DialogueInstances.singleton.ui.onDialogueEnd.RemoveListener(OnDialogueComplete);
			runner.Stop();
			HideAllDialogueButtons();
			DialogueInstances.singleton.ui.onDialogueEnd.AddListener(OnDialogueComplete);
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

			DialogueInstances.singleton.ui.onDialogueEnd.RemoveListener(OnDialogueComplete);
			(ItemType type, int index) = await ItemSelectionMenuUI.singleton.SelectItem(x => x.item.sellable);

			if (index != -1)
			{
				DialogueInstances.singleton.ui.onDialogueEnd.AddListener(OnDialogueComplete);
				OnSellMenuItemSelected(type, index);
			}//else the dialogue will handle it
			else if (!DialogueInstances.singleton.runner.IsDialogueRunning)
			{
				OnDialogueComplete();
			}
			else
			{
				DialogueInstances.singleton.ui.onDialogueEnd.AddListener(OnDialogueComplete);
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
			foreach (var button in DialogueInstances.singleton.ui.optionButtons)
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

				c.animationController.SetLookAtTarget(focus);


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
			NewItemPrompt.singleton.ShowPrompt(c.inventory.db[item], count, onComplete);
		}



		void ResetCamera()
		{
			GameCameras.s.conversationCamera.LookAt = GameCameras.s.conversationGroup.Transform;
		}


		#endregion
	}
}