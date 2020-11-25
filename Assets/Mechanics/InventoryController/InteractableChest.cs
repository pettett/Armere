using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableChest : ItemSpawnable, IInteractable
{

    public bool canInteract { get; set; } = true;

    public float requiredLookDot => -1;

    public string interactionDescription => "Open Chest";

    public Vector3 offset => throw new System.NotImplementedException();
    public event System.Action onChestOpened;

    public void Interact(IInteractor c)
    {

        Time.timeScale = 0;

        NewItemPrompt.singleton.ShowPrompt(item, count, () => { Time.timeScale = 1; });

        onChestOpened?.Invoke();

        //Do not allow multiple chest opens
        canInteract = false;
        gameObject.gameObject.transform.GetChild(0).localEulerAngles = new Vector3(0, 40, 0);

        //Do not drop items on destroy
        count = 0;
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

}