using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : PlayerRelativeObject, IInteractable
{
    public bool canInteract { get => enabled; set => enabled = value; }

    public System.Action<InteractableItem> onItemDestroy;

    ItemSpawner.SpawnType type;
    ItemName item;
    int count;
    ItemDatabase database;
    public void Init(ItemSpawner.SpawnType type, ItemName item, int count, ItemDatabase database)
    {
        this.type = type;
        this.item = item;
        this.count = count;
        this.database = database;
    }

    public void Interact(PlayerController.Player_CharacterController c)
    {
        if (type == ItemSpawner.SpawnType.Item)
        {
            DestroyItem();
            InventoryController.AddItem(item, 1);
        }
        else
        {
            NewItemPrompt.singleton.ShowPrompt(item, count, null);
            //Do not allow multiple chest opens
            enabled = false;
            gameObject.gameObject.transform.GetChild(0).localEulerAngles = new Vector3(0, 40, 0);
        }

        onItemDestroy?.Invoke(this);
    }

    public void OnStartHighlight()
    {
        //Show an arrow for this item with name on ui
        UIController.singleton.itemIndicator.StartIndication(transform,item.ToString());
    }

    public void OnEndHighlight()
    {
        //remove arrow
        UIController.singleton.itemIndicator.EndIndication();
    }

    void DestroyItem()
    {
        if (type == ItemSpawner.SpawnType.Item)
        {
            enabled = false;
            if (database[item].staticPickup)
                Spawner.DeSpawn(gameObject, ref Items.itemStaticPool);
            else
                Spawner.DeSpawn(gameObject, ref Items.itemPhysicsPool);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Disable()
    {
        DestroyItem();
    }
}
