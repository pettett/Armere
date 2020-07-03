using UnityEngine;

public class BuyInventoryUI : MonoBehaviour
{
    public GameObject template;
    public Transform inventoryDisplayHolder;
    public GameObject holder;

    BuyMenuItem[] inventory;
    int selected;
    public void ShowInventory(BuyMenuItem[] inventory, ItemDatabase db, System.Action onItemSelected)
    {
        print("Showing Buy Menu");
        this.inventory = inventory;
        holder.SetActive(true);
        for (int i = 0; i < inventory.Length; i++)
        {
            var item = Instantiate(template, inventoryDisplayHolder);
            var buyMenuItem = item.GetComponent<BuyInventoryUIItem>();
            buyMenuItem.title.text = inventory[i].item.ToString();
            buyMenuItem.stock.text = inventory[i].stock.ToString();
            buyMenuItem.thumbnail.sprite = db[inventory[i].item].sprite;
            buyMenuItem.cost.text = inventory[i].cost.ToString();

            int index = i;
            buyMenuItem.selectButton.onClick.AddListener(() =>
            {
                selected = index;
                onItemSelected?.Invoke();
            });
        }
    }

    public void ConfirmBuy()
    {
        print("Bought item " + selected.ToString());
        inventory[selected].stock--;
    }
    public void CancelBuy()
    {

    }
    public void CloseInventory()
    {
        holder.SetActive(false);
        for (int i = 0; i < inventoryDisplayHolder.childCount; i++)
        {
            Destroy(inventoryDisplayHolder.GetChild(i).gameObject);
        }
    }

}