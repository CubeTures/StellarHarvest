[System.Serializable]
public class InventorySlot
{
    public InventoryItem item;
    public int quantity;
    public bool IsEmpty => item == null || quantity <= 0;
}