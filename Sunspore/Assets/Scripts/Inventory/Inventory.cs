using System.Collections.Generic;

public class Inventory
{
    private List<InventorySlot> items = new();

    public void AddItem(ItemSO item)
    {
        InventorySlot slot = items.Find(i => i.item.Equals(item));
        if (slot != null)
        {
            slot.quantity += 1;
        }
        else
        {
            items.Add(new InventorySlot { item = item, quantity = 1 });
        }
    }

    public bool ContainsItem(ItemSO item)
    {
        return items.Find(i => i.item.Equals(item)) != null;
    }

    public void RemoveItem(ItemSO item)
    {
        InventorySlot slot = items.Find(i => i.item.Equals(item));
        if (slot != null)
        {
            items.Remove(slot);
        }
    }

    public void UseItem(ItemSO item)
    {
        int index = items.FindIndex(i => i.item.Equals(item));
        if (index >= 0)
        {
            items[index].quantity -= 1;
        }
    }

    public InventorySlot GetIndex(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            return null;
        }
        else
        {
            return items[index];
        }
    }

    public List<InventorySlot> GetAllSlots()
    {
        return new List<InventorySlot>(items);
    }
}
