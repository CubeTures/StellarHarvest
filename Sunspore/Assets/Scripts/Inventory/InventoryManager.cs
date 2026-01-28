using Sunspore;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{   
    public static InventoryManager Instance { get; private set; }
    public List<ItemTypeAndAmount> itemsToCraft = new List<ItemTypeAndAmount>();
    Inventory inventory;
    [SerializeField] Hotbar hotbar;
    [SerializeField] GameObject craftingParent;

    bool isCraftingOpened;

    void Awake()
    {
        // load inventory from some file
        inventory = new Inventory();
        hotbar.UpdateUI(inventory);
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        craftingParent.SetActive(isCraftingOpened);

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

    public List<ItemTypeAndAmount> GetAllItems()
    {
        // aggregate inventory slots into ItemTypeAndAmount list
        List<ItemTypeAndAmount> found = new List<ItemTypeAndAmount>();

        foreach (InventorySlot slot in inventory.GetAllSlots())
        {
            if (slot == null || slot.IsEmpty) continue;

            InventoryItem invItem = slot.item;
            int qty = slot.quantity;

            bool added = false;
            for (int i = 0; i < found.Count; i++)
            {
                if (found[i].item.Equals(invItem))
                {
                    found[i].amount += qty;
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                found.Add(new ItemTypeAndAmount(invItem, qty));
            }
        }

        return found;
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

    public void OpenAndCloseCrafting()
    {
        if (isCraftingOpened)
        {
            isCraftingOpened = false;
        }
        else
        {
            isCraftingOpened = true;
        }
    }
}