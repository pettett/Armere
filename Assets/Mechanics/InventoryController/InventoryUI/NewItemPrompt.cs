using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class NewItemPrompt : MonoBehaviour
{
    public static NewItemPrompt singleton;
    public ItemDatabase db;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    public void ShowPrompt(ItemName item, System.Action onPromptRemoved)
    {
        gameObject.SetActive(true);
        title.text = db[item].name;
        description.text = db[item].description;
        StartCoroutine(WaitForClosePrompt(onPromptRemoved));
    }
    IEnumerator WaitForClosePrompt(System.Action onPromptRemoved)
    {
        yield return new WaitForSeconds(1);
        gameObject.SetActive(false);
        onPromptRemoved?.Invoke();
    }
    private void Start()
    {
        gameObject.SetActive(false);
        singleton = this;
    }
}
