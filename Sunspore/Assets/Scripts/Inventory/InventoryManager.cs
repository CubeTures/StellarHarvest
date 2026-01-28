using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    Inventory inventory;
    [SerializeField] Hotbar hotbar;

    void Awake()
    {
        // load inventory from some file
        inventory = new Inventory();
        hotbar.UpdateUI(inventory);
    }

    void Update()
    {
        // navigate inventory
        if (Input.GetAxis("Mouse ScrollWheel") is float axis)
        {
            hotbar.ChangeSelected(axis);
            hotbar.UpdateUI(inventory);
        }

        // use item
        if (Input.GetKeyDown(KeyCode.E))
        {
            InventoryItem item = hotbar.GetSelectedItem(inventory);
            if (item != null)
            {
                UseItem(item);
            }
        }
    }

    public InventoryItem GetSelectedItem()
    {
        return hotbar.GetSelectedItem(inventory);
    }

    public void AddItem(InventoryItem item)
    {
        inventory.AddItem(item);
        hotbar.UpdateUI(inventory);
    }
    public bool ContainsItem(InventoryItem item)
    {
        return inventory.ContainsItem(item);
    }

    public void RemoveItem(InventoryItem item)
    {
        inventory.RemoveItem(item);
        hotbar.UpdateUI(inventory);
    }

    public void UseItem(InventoryItem item)
    {
        inventory.UseItem(item);
        hotbar.UpdateUI(inventory);
    }
}