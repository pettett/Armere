using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class NewItemPrompt : MonoBehaviour
{
    public static NewItemPrompt singleton;
    public GameObject holder;
    public ItemDatabase db;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    public void ShowPrompt(ItemName item, int count, System.Action onPromptRemoved)
    {
        holder.SetActive(true);
        title.text = db[item].name;
        description.text = db[item].description;
        InventoryController.AddItem(item, count);
        StartCoroutine(WaitForClosePrompt(onPromptRemoved));
    }
    IEnumerator WaitForClosePrompt(System.Action onPromptRemoved)
    {
        yield return new WaitForSeconds(1);
        holder.SetActive(false);
        onPromptRemoved?.Invoke();
    }
    private void Start()
    {
        holder.SetActive(false);
        singleton = this;
    }
}
