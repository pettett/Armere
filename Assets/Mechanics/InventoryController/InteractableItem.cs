using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : PlayerRelativeObject, IInteractable
{
    public bool canInteract { get => enabled; set => enabled = value; }

    public float requiredLookDot => -1;

    public System.Action<InteractableItem> onItemDestroy;

    public ItemSpawner.SpawnType type;
    ItemName item;
    uint count;

    public void Init(ItemSpawner.SpawnType type, ItemName item, uint count)
    {
        this.type = type;
        this.item = item;
        this.count = count;

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
        Items.DeSpawnItem(this);

    }

    public override void OnPlayerOutRange()
    {
        DestroyItem();
    }
}
