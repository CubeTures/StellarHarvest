using Sunspore;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot: MonoBehaviour
{
    public GameObject heldItem;
    public ItemSO item;
    public int quantity;
    public bool IsEmpty => item == null || quantity <= 0;
    Vector3 prevPosition;
    TooltipTrigger _tooltip;

    void Start()
    {
        prevPosition = transform.position;
        _tooltip = GetComponent<TooltipTrigger>();
        if (_tooltip == null)
            _tooltip = gameObject.AddComponent<TooltipTrigger>();
        RefreshTooltip();
    }

	public void SetHeldItem(GameObject item)
    {
        this.heldItem = item;
        Canvas.ForceUpdateCanvases();
        heldItem.transform.position = transform.position;
        RefreshTooltip();

        Log();
    }

    public void RefreshTooltip()
    {
        if (_tooltip == null) return;

        ItemSO itemData = null;
        if (heldItem != null)
        {
            var invItem = heldItem.GetComponent<InventoryItem>();
            if (invItem != null)
                itemData = invItem.itemScriptableObject;
        }

        if (itemData == null)
            itemData = item;

        if (itemData == null)
            _tooltip.SetContent("");
        else
            _tooltip.SetContent(itemData.displayName, itemData.description);
    }

    void Update()
    {
        if (heldItem != null)
        {
            heldItem.transform.position = transform.position;
        }
    }

    void Log()
    {

        if (heldItem != null)
        {
            // Debug.Log(heldItem.GetComponent<InventoryItem>().itemScriptableObject.displayName + " @ " + heldItem.transform.position + " vs " + transform.position);
        }
    }

    public ItemSO GetItemSO()
    {
        if (heldItem != null)
        {
            InventoryItem inventoryItem = heldItem.GetComponent<InventoryItem>();
            if (inventoryItem != null)
            {
                ItemSO itemSO = inventoryItem.itemScriptableObject;
                if (itemSO != null)
                {
                    return itemSO;
                }
            }
        }

        return null;
    }

    public int GetStackCount()
    {
        if (heldItem != null)
        {
            InventoryItem inventoryItem = heldItem.GetComponent<InventoryItem>();
            if (inventoryItem != null)
            {
                return inventoryItem.stackCurrent;
            }
        }

        return 0;
    }
}