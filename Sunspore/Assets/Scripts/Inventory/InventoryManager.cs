using Sunspore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static InventoryManager Instance { get; private set; }
    private List<ItemTypeAndAmount> items = new List<ItemTypeAndAmount>();
    Inventory inventory;
    [SerializeField] List<ItemSO> allItemScriptableObjects = new List<ItemSO>();

    [SerializeField] Hotbar hotbar;
    [SerializeField] GameObject craftingParent;
    [SerializeField] GameObject inventoryParent;
    [SerializeField] Transform handParent;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] Camera cam;

    [SerializeField] GameObject[] slots = new GameObject[12];
    [SerializeField] GameObject[] hotbarSlots = new GameObject[3];

    bool isCraftingOpened;
    bool isInventoryOpened;
    int selectedHotbarIndex = 0;
    GameObject draggedObject;
    GameObject lastItemSlot;
    bool hasLoggedMissingHandParent = false;
    void Awake()
    {
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

    void Start()
    {
        AutoPopulateHeldItems();
        HotBarItemChanged();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        CheckForHotbarInput();
        craftingParent.SetActive(isCraftingOpened);
        inventoryParent.SetActive(isInventoryOpened);
        // // navigate inventory
        // if (Input.GetAxis("Mouse ScrollWheel") is float axis)
        // {
        //     hotbar.ChangeSelected(axis);
        //     hotbar.UpdateUI(inventory);
        // }
        if (draggedObject != null)
        {
            draggedObject.transform.position = Input.mousePosition;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isInventoryOpened)
            {
                isInventoryOpened = false;
                isCraftingOpened = false;
            }
            else
            {
                isInventoryOpened = true;
            }
        }
    }

    private void CheckForHotbarInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedHotbarIndex = 0;
            HotBarItemChanged();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedHotbarIndex = 1;
            HotBarItemChanged();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedHotbarIndex = 2;
            HotBarItemChanged();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            selectedHotbarIndex = 3;
            HotBarItemChanged();
        }
    }

    private void HotBarItemChanged()
    {
        if (hotbarSlots == null || hotbarSlots.Length == 0) return;

        if (selectedHotbarIndex < 0) selectedHotbarIndex = 0;
        if (selectedHotbarIndex >= hotbarSlots.Length) selectedHotbarIndex = hotbarSlots.Length - 1;

        if (handParent == null)
        {
            if (!hasLoggedMissingHandParent)
            {
                Debug.LogWarning("InventoryManager: 'handParent' is not assigned in the inspector. Hand visuals will be disabled.");
                hasLoggedMissingHandParent = true;
            }
        }
        else
        {
            for (int i = 0; i < handParent.childCount; i++)
            {
                handParent.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (WeaponManager.Instance != null)
        {
            var sel = GetSelectedItem();
            if (sel == null || !sel.weapon)
            {
                WeaponManager.Instance.enabled = false;
            }
        }

        foreach (GameObject slot in hotbarSlots)
        {
            if (slot == null) continue;

            Vector3 scale = new Vector3(0.9f, 0.9f, 0.9f);

            if (slot == hotbarSlots[selectedHotbarIndex])
            {
                scale = new Vector3(1.1f, 1.1f, 1.1f);

                InventorySlot invSlot = slot.GetComponent<InventorySlot>();
                var selected = GetSelectedItem();
                if (selected != null && selected.weapon)
                {
                    if (WeaponManager.Instance != null)
                    {
                        WeaponManager.Instance.SetWeaponStats();
                        WeaponManager.Instance.enabled = true;
                    }
                }
                if (invSlot != null && invSlot.heldItem != null && handParent != null)
                {
                    Debug.Log("we are in");
                    var heldInvItem = invSlot.heldItem.GetComponent<InventoryItem>();
                    if (heldInvItem != null)
                    {
                        for (int i = 0; i < handParent.childCount; i++)
                        {
                            var handChild = handParent.GetChild(i).GetComponent<InventoryItem>();
                            if (handChild != null && handChild.itemScriptableObject == heldInvItem.itemScriptableObject)
                            {
                                handParent.GetChild(i).gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }

            slot.transform.localScale = scale;
        }
    }

    private void AutoPopulateHeldItems()
    {
        if (itemPrefab == null) return;

        // populate inventory slots
        foreach (GameObject slotObj in slots)
        {
            if (slotObj == null) continue;

            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            if (slot == null) continue;

            if (slot.item != null && slot.heldItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab);
                var invItem = newItem.GetComponent<InventoryItem>();
                if (invItem != null)
                {
                    invItem.itemScriptableObject = slot.item;
                    invItem.stackCurrent = slot.quantity > 0 ? slot.quantity : 1;
                }
                newItem.transform.SetParent(slotObj.transform.parent.parent.GetChild(2));
                newItem.transform.localScale = new Vector3(1, 1, 1);
                slot.SetHeldItem(newItem);
            }
        }

        // populate hotbar slots
        foreach (GameObject slotObj in hotbarSlots)
        {
            if (slotObj == null) continue;

            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            if (slot == null) continue;

            if (slot.item != null && slot.heldItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab);
                var invItem = newItem.GetComponent<InventoryItem>();
                if (invItem != null)
                {
                    invItem.itemScriptableObject = slot.item;
                    invItem.stackCurrent = slot.quantity > 0 ? slot.quantity : 1;
                }
                newItem.transform.SetParent(slotObj.transform.parent.parent.GetChild(2));
                newItem.transform.localScale = new Vector3(1, 1, 1);
                slot.SetHeldItem(newItem);
            }
        }
    }

    public List<ItemTypeAndAmount> GetAllItems()
    {
        items.Clear();

        foreach (GameObject slot in slots)
        {
            InventorySlot slotScript = slot.GetComponent<InventorySlot>();

            if (slotScript.heldItem != null)
            {
                InventoryItem itemScript = slotScript.heldItem.GetComponent<InventoryItem>();

                int i = 0;
                bool wasItemAdded = false;

                foreach (ItemTypeAndAmount itemAndCount in items)
                {
                    if (itemAndCount.item == itemScript.itemScriptableObject)
                    {
                        items[i].amount += itemScript.stackCurrent;
                        wasItemAdded = true;
                    }
                    i++;
                }

                if (!wasItemAdded)
                {
                    items.Add(new ItemTypeAndAmount(itemScript.itemScriptableObject, itemScript.stackCurrent));
                }
            }
        }

        foreach (GameObject slot in hotbarSlots)
        {
            InventorySlot slotScript = slot.GetComponent<InventorySlot>();

            if (slotScript.heldItem != null)
            {
                InventoryItem itemScript = slotScript.heldItem.GetComponent<InventoryItem>();

                int i = 0;
                bool wasItemAdded = false;

                foreach (ItemTypeAndAmount itemAndCount in items)
                {
                    if (itemAndCount.item == itemScript.itemScriptableObject)
                    {
                        items[i].amount += itemScript.stackCurrent;
                        wasItemAdded = true;
                    }
                    i++;
                }

                if (!wasItemAdded)
                {
                    items.Add(new ItemTypeAndAmount(itemScript.itemScriptableObject, itemScript.stackCurrent));
                }
            }
        }
        return items;
    }

    // public void AddItem(ItemSO item)
    // {
    //     inventory.AddItem(item);
    //     hotbar.UpdateUI(inventory);
    // }
    // public bool ContainsItem(ItemSO item)
    // {
    //     return inventory.ContainsItem(item);
    // }

    // public void RemoveItem(ItemSO item)
    // {
    //     inventory.RemoveItem(item);
    //     hotbar.UpdateUI(inventory);
    // }

    public void UseSelectedItem()
    {
        if (hotbarSlots == null || hotbarSlots.Length == 0)
            return;

        if (selectedHotbarIndex < 0 || selectedHotbarIndex >= hotbarSlots.Length)
            return;

        InventorySlot slot = hotbarSlots[selectedHotbarIndex].GetComponent<InventorySlot>();
        if (slot == null || slot.heldItem == null)
            return;

        InventoryItem invItem = slot.heldItem.GetComponent<InventoryItem>();
        if (invItem == null)
            return;

        ItemSO itemSO = GetSelectedItem();
        if (itemSO == null)
            return;

        invItem.stackCurrent--;

        if (invItem.stackCurrent <= 0)
        {
            Destroy(slot.heldItem);
            slot.heldItem = null;
        }

        HotBarItemChanged();
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
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();

            if (slot != null && slot.heldItem != null)
            {
                draggedObject = slot.heldItem;
                slot.heldItem = null;
                lastItemSlot = clickedObject;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (draggedObject != null && eventData.pointerCurrentRaycast.gameObject != null && eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();

            //There isnt item in the slot - place item
            if (slot != null && slot.heldItem == null)
            {
                slot.SetHeldItem(draggedObject);
                draggedObject.transform.SetParent(slot.transform.parent.parent.GetChild(2));
            }
            //There is item in the slot - switch items
            else if (slot != null && slot.heldItem != null && slot.heldItem.GetComponent<InventoryItem>().stackCurrent == slot.heldItem.GetComponent<InventoryItem>().stackMax
                || slot != null && slot.heldItem != null && slot.heldItem.GetComponent<InventoryItem>().itemScriptableObject != draggedObject.GetComponent<InventoryItem>().itemScriptableObject)
            {
                lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(slot.heldItem);
                slot.heldItem.transform.SetParent(slot.transform.parent.parent.GetChild(2));

                slot.SetHeldItem(draggedObject);
                draggedObject.transform.SetParent(slot.transform.parent.parent.GetChild(2));
            }

            //Fill stack
            else if (slot != null && slot.heldItem != null && slot.heldItem.GetComponent<InventoryItem>().stackCurrent < slot.heldItem.GetComponent<InventoryItem>().stackMax
                && slot.heldItem.GetComponent<InventoryItem>().itemScriptableObject == draggedObject.GetComponent<InventoryItem>().itemScriptableObject)
            {
                InventoryItem slotHeldItem = slot.heldItem.GetComponent<InventoryItem>();
                InventoryItem draggedItem = draggedObject.GetComponent<InventoryItem>();

                int itemsToFillStack = slotHeldItem.stackMax - slotHeldItem.stackCurrent;

                if (itemsToFillStack >= draggedItem.stackCurrent)
                {
                    slotHeldItem.stackCurrent += draggedItem.stackCurrent;
                    Destroy(draggedObject);
                }
                else
                {
                    slotHeldItem.stackCurrent += itemsToFillStack;
                    draggedItem.stackCurrent -= itemsToFillStack;
                    lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(draggedObject);
                }
            }
            //Return item to last slot
            else if (clickedObject.name != "DropItem")
            {
                lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(draggedObject);
                draggedObject.transform.SetParent(slot.transform.parent.parent.GetChild(2));
            }
            //Drop item
            else
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Vector3 position = ray.GetPoint(3);

                GameObject newItem = Instantiate(draggedObject.GetComponent<InventoryItem>().itemScriptableObject.prefab, position, new Quaternion());
                newItem.GetComponent<itemPickable>().itemScriptableObject = draggedObject.GetComponent<InventoryItem>().itemScriptableObject;

                lastItemSlot.GetComponent<InventorySlot>().heldItem = null;
                Destroy(draggedObject);
            }

            HotBarItemChanged();
            draggedObject = null;
        }
    }

    public void ItemPicked(GameObject pickedItem)
    {
        GameObject emptySlot = null;

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i].GetComponent<InventorySlot>();

            if (slot.heldItem == null)
            {
                emptySlot = slots[i];
                break;
            }
        }

        if (emptySlot != null)
        {
            GameObject newItem = Instantiate(itemPrefab);
            newItem.GetComponent<InventoryItem>().itemScriptableObject = pickedItem.GetComponent<itemPickable>().itemScriptableObject;
            newItem.transform.SetParent(emptySlot.transform.parent.parent.GetChild(2));
            newItem.GetComponent<InventoryItem>().stackCurrent = 1;

            emptySlot.GetComponent<InventorySlot>().SetHeldItem(newItem);
            newItem.transform.localScale = new Vector3(1, 1, 1);

            Destroy(pickedItem);
        }
    }

    public ItemSO GetSelectedItem()
    {
        if (hotbarSlots == null || hotbarSlots.Length == 0)
            return null;

        if (selectedHotbarIndex < 0 || selectedHotbarIndex >= hotbarSlots.Length)
            return null;

        InventorySlot slot = hotbarSlots[selectedHotbarIndex].GetComponent<InventorySlot>();
        if (slot == null || slot.heldItem == null)
            return null;

        // Get the inventory item script to check stack count
        InventoryItem itemScript = slot.heldItem.GetComponent<InventoryItem>();

        // Return null if script is missing OR stack is 0 or less
        if (itemScript == null || itemScript.stackCurrent <= 0)
            return null;

        return itemScript.itemScriptableObject;
    }
    public void CraftItem(List<ItemTypeAndAmount> itemsToCraft, List<ItemTypeAndAmount> itemsToDestroy)
    {
        foreach (ItemTypeAndAmount itemToCraft in itemsToCraft)
        {
            GameObject emptySlot = null;

            for (int i = 0; i < slots.Length; i++)
            {
                InventorySlot slot = slots[i].GetComponent<InventorySlot>();

                if (slot.heldItem == null)
                {
                    emptySlot = slots[i];
                    break;
                }
            }

            if (emptySlot != null)
            {
                GameObject newItem = Instantiate(itemPrefab);
                newItem.GetComponent<InventoryItem>().itemScriptableObject = itemToCraft.item;
                newItem.transform.SetParent(emptySlot.transform.parent.parent.GetChild(2));
                newItem.GetComponent<InventoryItem>().stackCurrent = itemToCraft.amount;

                emptySlot.GetComponent<InventorySlot>().SetHeldItem(newItem);
                newItem.transform.localScale = new Vector3(1, 1, 1);
            }   
        }
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetComponent<InventorySlot>().heldItem != null)
            {
                InventorySlot slotScript = slots[i].GetComponent<InventorySlot>();
                InventoryItem itemScript = slotScript.heldItem.GetComponent<InventoryItem>();

                foreach (ItemTypeAndAmount itemToDestroy in itemsToDestroy)
                {
                    if (itemToDestroy.item == itemScript.itemScriptableObject)
                    {
                        if (itemToDestroy.amount >= itemScript.stackCurrent)
                        {
                            slotScript.heldItem = null;
                            Destroy(itemScript.gameObject);
                        }
                        else if (itemToDestroy.amount < itemScript.stackCurrent)
                        {
                            itemScript.stackCurrent -= itemToDestroy.amount;
                        }
                    }
                }
            }
        }
    }

    public void AddItem(ItemSO itemToAdd, int amount)
    {
        GameObject emptySlot = null;

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i].GetComponent<InventorySlot>();

            if (slot.heldItem == null)
            {
                emptySlot = slots[i];
                break;
            }
        }

        if (emptySlot != null)
        {
            GameObject newItem = Instantiate(itemPrefab);
            newItem.GetComponent<InventoryItem>().itemScriptableObject = itemToAdd;
            newItem.transform.SetParent(emptySlot.transform.parent.parent.GetChild(2));
            newItem.GetComponent<InventoryItem>().stackCurrent = amount;

            emptySlot.GetComponent<InventorySlot>().SetHeldItem(newItem);
            newItem.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}