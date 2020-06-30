using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class NewItemPrompt : MonoBehaviour
{
    public static NewItemPrompt singleton;
    public GameObject holder;
    public ItemDatabase db;
    public Image thumbnail;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    public void ShowPrompt(ItemName item, uint count, System.Action onPromptRemoved)
    {
        if (count == 0)
        {
            Debug.LogWarning("Giving prompt for 0 items!");
            return;
        }

        holder.SetActive(true);

        if (count == 1)
            title.text = db[item].name;
        else //Tell the player how many items they are getting
            title.text = string.Format("{0} x{1}", db[item].name, count);

        description.text = db[item].description;

        thumbnail.sprite = db[item].sprite;

        InventoryController.AddItem(item, count);



        StartCoroutine(WaitForClosePrompt(onPromptRemoved));
    }
    IEnumerator WaitForClosePrompt(System.Action onPromptRemoved)
    {
        //force show the prompt for a short time
        yield return new WaitForSecondsRealtime(1);
        //then wait for user to remove it


        var continueAction = new InputAction(binding: "/*/<button>");

        continueAction.performed += (context) =>
        {
            //Remove the prompt
            continueAction.Dispose();
            holder.SetActive(false);
            onPromptRemoved?.Invoke();
        };
        continueAction.Enable();

    }
    private void Start()
    {
        holder.SetActive(false);
        singleton = this;
    }
}
