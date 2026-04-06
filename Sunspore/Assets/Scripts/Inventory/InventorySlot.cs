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

    void Start()
    {
        prevPosition = transform.position;
    }

	public void SetHeldItem(GameObject item)
    {
        this.heldItem = item;
        Canvas.ForceUpdateCanvases();
        heldItem.transform.position = transform.position;

        Log();
    }

    void Update()
    {
        if (heldItem != null)
        {
            heldItem.transform.position = transform.position;
        }
        // Log();
    }

    void Log()
    {

        if (heldItem != null)
        {
            Debug.Log(heldItem.GetComponent<InventoryItem>().itemScriptableObject.displayName + " @ " + heldItem.transform.position + " vs " + transform.position);
        }
    }
}