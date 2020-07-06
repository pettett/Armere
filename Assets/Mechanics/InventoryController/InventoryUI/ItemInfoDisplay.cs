using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ItemInfoDisplay : MonoBehaviour
{
    public Image thumbnail;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public void ShowInfo(ItemName item, ItemDatabase db){
        thumbnail.sprite = db[item].sprite;
        title.text = db[item].name;
        description.text = db[item].description;
    }
}