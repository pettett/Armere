using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestComponent : WorldObjectComponent, IInteractable
{
    public bool canInteract { get; set; } = true;

    public float requiredLookDot => 0;

    public string interactionDescription => "Chest";

    public void Interact(IInteractor interactor)
    {
        Time.timeScale = 0;

        NewItemPrompt.singleton.ShowPrompt(worldObject.instanceData.containsItem, worldObject.instanceData.containsItemCount, () => { Time.timeScale = 1; });
        //Do not allow multiple chest opens
        canInteract = false;
        gameObject.gameObject.transform.GetChild(0).localEulerAngles = new Vector3(0, 40, 0);

        //Leave no items inside the chest (in case it is later destroyed)
        worldObject.instanceData = default;
    }
    private void OnDestroy()
    {
        canInteract = false;
    }

    public void OnEndHighlight()
    {
        UIController.singleton.itemIndicator.EndIndication();
    }

    public void OnStartHighlight()
    {
        UIController.singleton.itemIndicator.StartIndication(
            transform, "Open Chest");
    }

}
