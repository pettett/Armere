using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : PlayerRelativeObject, IInteractable
{
    public bool canInteract { get => enabled; set => enabled = value; }

    public System.Action<InteractableItem> onItemDestroy;

    ItemSpawner.SpawnType type;
    ItemName item;
    uint count;
    ItemDatabase database;
    public void Init(ItemSpawner.SpawnType type, ItemName item, uint count, ItemDatabase database)
    {
        this.type = type;
        this.item = item;
        this.count = count;
        this.database = database;
        AddToRegister();
    }

    public void Interact(IInteractor c)
    {
        if (type == ItemSpawner.SpawnType.Item)
        {
            DestroyItem();
            InventoryController.AddItem(item, 1);
        }
        else
        {
            Time.timeScale = 0;

            NewItemPrompt.singleton.ShowPrompt(item, count, () => { Time.timeScale = 1; });
            //Do not allow multiple chest opens
            enabled = false;
            gameObject.gameObject.transform.GetChild(0).localEulerAngles = new Vector3(0, 40, 0);
        }

        onItemDestroy?.Invoke(this);
    }

    public void OnStartHighlight()
    {
        //Show an arrow for this item with name on ui
        UIController.singleton.itemIndicator.StartIndication(transform, item.ToString());
    }

    public void OnEndHighlight()
    {
        //remove arrow
        UIController.singleton.itemIndicator.EndIndication();
    }

    void DestroyItem()
    {
        RemoveFromRegister();
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

    public override void OnPlayerOutRange()
    {
        DestroyItem();
    }
}
