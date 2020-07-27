using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerController;
using Yarn.Unity;

using Cinemachine;
using Yarn;

public class NPC : AIBase, IInteractable, IVariableAddon
{
    public bool canInteract { get => enabled; set => enabled = value; }

    public string prefix => "$NPC_";



    public Value this[string name]
    {
        //Im sure there are many reasons why this is terrible, but yarn variables are not serializeable so cannot be saved
        get => NPCManager.NPCVariable.ToYarnEquiv(NPCManager.singleton.data[npcName].variables[name]);
        set => NPCManager.singleton.data[npcName].variables[name] = NPCManager.NPCVariable.FromYarnEquiv(value);
    }

    public NPCName npcName;
    public Transform ambientThought;
    new Camera camera;
    NPCTemplate t;
    Transform[] conversationGroupOverride;
    Conversation currentConv;
    public static Dictionary<NPCName, NPC> activeNPCs = new Dictionary<NPCName, NPC>();

    public Animator animator;
    bool hasSpeaker = false;
    NPCName speakingNPC;

    NPCSpawn spawn;


    public BuyMenuItem[] buyInventory;
    public void InitNPC(NPCTemplate template, NPCSpawn spawn, Transform[] conversationGroupOverride)
    {
        t = template;

        this.spawn = spawn;
        //Copy the buy inventory
        buyInventory = new BuyMenuItem[t.buyMenuItems.Length];
        for (int i = 0; i < buyInventory.Length; i++)
        {
            //Copy all the data
            buyInventory[i].item = t.buyMenuItems[i].item;
            buyInventory[i].cost = t.buyMenuItems[i].cost;
            buyInventory[i].count = t.buyMenuItems[i].count;
            buyInventory[i].stock = t.buyMenuItems[i].stock;
        }

        npcName = spawn.spawnedNPCName;
        activeNPCs[npcName] = this;
        speakingNPC = npcName;
        this.conversationGroupOverride = conversationGroupOverride;
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();

        if (!NPCManager.singleton.data.ContainsKey(npcName))
        {
            //Only add this data if the NPC has not existed in the save before
            NPCManager.singleton.data[npcName] = new NPCManager.NPCData(t);
        }
    }

    private void Awake()
    {
        camera = Camera.main;
    }

    DialogueRunner runner => NPCManager.singleton.dialogueRunner;

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
    public void SetupRunner()
    {

        runner.Add(t.dialogue);
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
        DialogueUI.singleton.onLineStart.RemoveListener(StartNPCSpeaking);

        (runner.variableStorage as InMemoryVariableStorage).addon = null;
    }

    void SetDialogBoxActive(bool active) => DialogueInstances.singleton.dialogueUI.dialogueContainer.SetActive(active);
    void EnableDialogBox() => SetDialogBoxActive(true);
    void DisableDialogBox() => SetDialogBoxActive(false);

    void GoTo(string[] arg, System.Action onComplete)
    {
        DisableDialogBox();
        GoToPosition(spawn.walkingPoints[arg[0]].position,
        () =>
        {
            //Re-enable the dialog box when the AI has finished walking
            EnableDialogBox();
            onComplete?.Invoke();
        }
        );
    }
    BuyInventoryUI buyMenu;
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
        buyMenu.ShowInventory(buyInventory, InventoryController.singleton.db, OnBuyMenuItemSelected);
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
        currentConv.RunSellMenu(OnSellMenuItemSelected);
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
        currentConv.CloseSellMenu();
    }


    public void StartNPCSpeaking(string line)
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
            activeNPCs[currentSpeaker].StartSpeaking(c, true);
            hasSpeaker = true;
        }
        else if (speakingNPC != currentSpeaker)
        {
            activeNPCs[speakingNPC].StopSpeaking();
            activeNPCs[currentSpeaker].StartSpeaking(c, false);
        }
        speakingNPC = currentSpeaker;
    }




    IEnumerator RotatePlayerTowardsNPC(Transform playerTransform)
    {
        Quaternion desiredRot = Quaternion.LookRotation(transform.position - playerTransform.position);
        while (Quaternion.Angle(desiredRot, playerTransform.rotation) > 1f)
        {
            playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, desiredRot, Time.deltaTime * 800);
            yield return new WaitForEndOfFrame();
        }
    }


    public void StartSpeaking(Player_CharacterController c, bool first)
    {
        if (!first)
            //Make the camera look at this npc
            PointCameraToSpeaker(c);
        else
            c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = GetCameraAngle(c);


        //Point the player towards the currently speaking npc
        StartCoroutine(RotatePlayerTowardsNPC(c.transform));


        foreach (var r in GetComponentsInChildren<Renderer>())
            foreach (var mat in r.materials)
                mat.color = Color.blue;
    }
    ///<summary>Point the player's conversation camera to this npc</summary>
    void PointCameraToSpeaker(Player_CharacterController c)
    {
        //Get an angle that looks from the player to the npc, at a slight offset
        c.conversationGroup.Transform.rotation = Quaternion.Euler(0, GetCameraAngle(c), 0);
        //Setup the transposer to recenter
        c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.CancelRecentering();
        c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = true;
    }


    public void StopSpeaking()
    {
        StopAllCoroutines();
        foreach (var r in GetComponentsInChildren<Renderer>())
            foreach (var mat in r.materials)
                mat.color = Color.red;
    }

    Player_CharacterController c;

    public float GetCameraAngle(Player_CharacterController c)
    {
        return Quaternion.LookRotation(transform.position - c.transform.position).eulerAngles.y + 15f;
    }

    public void Interact(IInteractor interactor)
    {
        SetupRunner();

        this.c = (interactor as Player_CharacterController);
        currentConv = c.ChangeToState<Conversation>();

        //Probably quicker to overrite true with true then find the value and
        //check if it is true then find it again to set it
        NPCManager.singleton.data[npcName].spokenTo = true;


        c.conversationGroup.Transform.position = Vector3.Lerp(c.transform.position, transform.position, 0.5f);

        int targets = Mathf.Max(2, conversationGroupOverride.Length + 1);
        //Add all targets including the player
        c.conversationGroup.m_Targets = new CinemachineTargetGroup.Target[targets];
        c.conversationGroup.m_Targets[0] = GenerateTarget(c.transform);
        if (conversationGroupOverride.Length != 0)
        {
            for (int i = 0; i < conversationGroupOverride.Length; i++)
            {
                c.conversationGroup.m_Targets[i + 1] = GenerateTarget(conversationGroupOverride[i]);
            }
        }
        else
        {
            c.conversationGroup.m_Targets[1] = GenerateTarget(transform);
        }

        c.conversationGroup.DoUpdate();

        //Add the variables for this NPC

        (runner.variableStorage as InMemoryVariableStorage).addon = this;

        c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = false;
        //  c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = GetCameraAngle(c);

        StartCoroutine(TurnToPlayer(c.transform.position));
    }


    void OnDialogueComplete()
    {
        //Conversation over - clean up
        activeNPCs[speakingNPC].StopSpeaking();
        hasSpeaker = false;
        c.ChangeToState<Walking>();

        ResetCamera();

        CleanUpRunner();
    }


    CinemachineTargetGroup.Target GenerateTarget(Transform transform, float weight = 1, float radius = 1)
    {
        return new CinemachineTargetGroup.Target() { target = transform, weight = weight, radius = radius };
    }



    Vector3 focusPoint;
    IEnumerator TurnToPlayer(Vector3 playerPosition)
    {
        DialogueUI.singleton.onLineStart.AddListener(StartNPCSpeaking);
        DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);

        runner.StartDialogue("Start");

        Quaternion desiredRot = Quaternion.LookRotation(playerPosition - transform.position);
        while (Quaternion.Angle(desiredRot, transform.rotation) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, Time.deltaTime * 800);
            yield return new WaitForEndOfFrame();
        }


    }



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
        for (int i = 0; i < t.quests.Length; i++)
        {
            if (t.quests[i].name == questName)
            {
                QuestManager.AddQuest(t.quests[i]);
            }
        }
    }
    void DeliverQuest(string[] arg) => QuestManager.ForfillDeliverQuest(arg[0]);

    void TalkToQuest(string[] arg) => QuestManager.ForfillTalkToQuest(arg[0]);


    private void CameraPan(string[] arg, System.Action onComplete)
    {
        if (spawn.focusPoints.ContainsKey(arg[0]))
            //pan the camera to the target destination
            StartCoroutine(TurnCameraToTarget(spawn.focusPoints[arg[0]].position, onComplete));
        else
        {
            Debug.LogWarning("Lookat target not in dictionary");
            onComplete?.Invoke();
        }
    }


    void TurnPlayerToTarget(string[] arg, System.Action onComplete)
    {
        Vector3 target = spawn.focusPoints[arg[0]].position;
        target.y = c.transform.position.y;
        c.transform.LookAt(target);
        onComplete?.Invoke();
    }
    void TurnNPCToTarget(string[] arg, System.Action onComplete)
    {
        Vector3 target = spawn.focusPoints[arg[0]].position;
        target.y = transform.position.y;
        transform.LookAt(target);
        onComplete?.Invoke();
    }
    void TurnNPCAndPlayerToTarget(string[] arg, System.Action onComplete)
    {
        if (spawn.focusPoints.ContainsKey(arg[0]))
        {
            Vector3 target = spawn.focusPoints[arg[0]].position;
            target.y = transform.position.y;
            transform.LookAt(target);
            target.y = c.transform.position.y;
            c.transform.LookAt(target);
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

    IEnumerator TurnCameraToTarget(Vector3 target, System.Action onComplete)
    {
        //Orbit around the focus point
        //
        Quaternion targetRotation = Quaternion.LookRotation((target - focusPoint + Vector3.up));

        //Vector3 pos = focusPoint + Vector3.up + targetRotation * Vector3.back * 2;

        //yield return LerpCameraToPositionAndRotation(pos, targetRotation, 0.3f);

        c.cutsceneCamera.LookAt = c.lookAtTarget;
        c.lookAtTarget.position = target;

        yield return new WaitForSeconds(0.5f);

        onComplete?.Invoke();
    }

    void ResetCamera()
    {
        c.cutsceneCamera.LookAt = c.conversationGroup.Transform;
    }


    IEnumerator LerpCameraToPositionAndRotation(Vector3 targetPosition, Quaternion targetRotation, float time)
    {
        //Orbit around the focus point
        Vector3 vel = Vector3.zero;
        float angleVel = 0;
        float delta;
        float t = 0;
        float elapsedTime = 0;
        Quaternion startRot = camera.transform.rotation;
        do
        {
            elapsedTime += Time.deltaTime * 0.25f;
            delta = Quaternion.Angle(camera.transform.rotation, targetRotation);
            t = Mathf.SmoothDampAngle(t, 1, ref angleVel, time, Mathf.Infinity, Time.deltaTime);

            camera.transform.SetPositionAndRotation(
                Vector3.SmoothDamp(camera.transform.position, targetPosition, ref vel, time, Mathf.Infinity, Time.deltaTime),
                Quaternion.Slerp(startRot, targetRotation, t)
            );

            yield return new WaitForEndOfFrame();
        } while ((delta > 0.1 || (camera.transform.position - targetPosition).sqrMagnitude > 0.01f) && elapsedTime < time);

        camera.transform.SetPositionAndRotation(targetPosition, targetRotation);
    }


    private void Update()
    {
        ambientThought.rotation = camera.transform.rotation;
    }

    public void OnStartHighlight()
    {
        //show arrow
        UIController.singleton.npcIndicator.StartIndication(
            transform,
            NPCManager.singleton.data[npcName].spokenTo ? npcName.ToString() : "", //Do not show the npc name if the player has never spoken to them
            Vector3.up * 2);
    }

    public void OnEndHighlight()
    {
        //remove arrow
        UIController.singleton.npcIndicator.EndIndication();
    }
}
