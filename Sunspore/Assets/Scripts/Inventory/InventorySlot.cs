using UnityEngine;

public class InventorySlot: MonoBehaviour
{
    public GameObject heldItem;
    public ItemSO item;
    public int quantity;
    public bool IsEmpty => item == null || quantity <= 0;

    public void SetHeldItem(GameObject item)
    {
        this.heldItem = item;
        heldItem.transform.position = transform.position;
    }
}