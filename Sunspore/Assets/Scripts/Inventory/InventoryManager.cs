using Sunspore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static InventoryManager Instance { get; private set; }
    private List<ItemTypeAndAmount> items = new List<ItemTypeAndAmount>();
    Inventory inventory;
    [SerializeField] Hotbar hotbar;
    [SerializeField] GameObject craftingParent;
    [SerializeField] GameObject debtParent;
    [SerializeField] GameObject milestoneParent;
    [SerializeField] GameObject inventoryParent;
    [SerializeField] GameObject storeParent;
    [SerializeField] Transform handParent;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] Camera cam;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] RectTransform itemsContainer;
    [SerializeField] Sprite hotbarSlotUnselected, hotbarSlotSelected;

    [SerializeField] GameObject[] slots = new GameObject[28];
    [SerializeField] GameObject[] hotbarSlots = new GameObject[7];
    [SerializeField] HandHeldManager handHeldManager;
    [SerializeField] ToggleButtonGroup inventoryTabs;

    bool isCraftingOpened;
    public bool isInventoryOpened;
    bool isStoreOpened;
    bool isDebtOpened;
    bool isMilestoneOpened;
    // Inventory filtering
    private ItemCategory selectedInventoryCategory = ItemCategory.All;
    int selectedHotbarIndex = 0;
    GameObject draggedObject;
    GameObject lastItemSlot;
    bool hasLoggedMissingHandParent = false;
    public bool pointerDown = false;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
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
        debtParent.SetActive(isDebtOpened);
        inventoryParent.SetActive(isInventoryOpened);
        if (isInventoryOpened)
        {
            UpdateInventoryFilter();
        }
        if (storeParent != null)
        {
            storeParent.SetActive(isStoreOpened);
        }
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
                isStoreOpened = false;
                isDebtOpened = false;
                isMilestoneOpened = false;
                inventoryTabs.Reset();
            }
            else
            {
                isInventoryOpened = true;
                PlayerPrefManager.SetInventoryOpenCount(PlayerPrefManager.GetInventoryOpenCount() + 1);
            }
        }
        itemsContainer.anchoredPosition = scrollRect.content.anchoredPosition;
    }

    private void UpdateInventoryFilter()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            var slotObj = slots[i];
            if (slotObj == null) continue;

            var slot = slotObj.GetComponent<InventorySlot>();
            if (slot == null || slot.heldItem == null) continue;

            var invItem = slot.heldItem.GetComponent<InventoryItem>();
            if (invItem == null || invItem.itemScriptableObject == null) continue;

            bool show = selectedInventoryCategory == ItemCategory.All || invItem.itemScriptableObject.category == selectedInventoryCategory;
            slot.heldItem.SetActive(show);
        }
    }

    public void SelectedInventoryCategory(string type)
    {
        if (string.IsNullOrEmpty(type))
        {
            selectedInventoryCategory = ItemCategory.All;
        }
        else
        {
            switch (type.ToLowerInvariant())
            {
                case "weapons":
                    selectedInventoryCategory = ItemCategory.Weapons;
                    break;
                case "seeds":
                    selectedInventoryCategory = ItemCategory.Seeds;
                    break;
                case "plants":
                    selectedInventoryCategory = ItemCategory.Plants;
                    break;
                case "tools":
                    selectedInventoryCategory = ItemCategory.Tools;
                    break;
                default:
                    selectedInventoryCategory = ItemCategory.All;
                    break;
            }
        }
        // Apply filter immediately
        UpdateInventoryFilter();
    }

    public void SelectedInventoryCategory_Weapons() { SelectedInventoryCategory("weapons"); }
    public void SelectedInventoryCategory_Seeds() { SelectedInventoryCategory("seeds"); }
    public void SelectedInventoryCategory_Plants() { SelectedInventoryCategory("plants"); }
    public void SelectedInventoryCategory_Tools() { SelectedInventoryCategory("tools"); }
    public void SelectedInventoryCategory_All() { SelectedInventoryCategory(""); }

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
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            selectedHotbarIndex = 4;
            HotBarItemChanged();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            selectedHotbarIndex = 5;
            HotBarItemChanged();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            selectedHotbarIndex = 6;
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

        var sel = GetSelectedItem();

        if (handHeldManager != null)
        {
            handHeldManager.CancelAbility();
            if (sel == null || (sel.category != ItemCategory.Weapons && sel.category != ItemCategory.Tools))
            {
                handHeldManager.enabled = false;
            }
        }

        foreach (GameObject slot in hotbarSlots)
        {
            if (slot == null) continue;

            Sprite spr = hotbarSlotUnselected;

            if (slot == hotbarSlots[selectedHotbarIndex])
            {
                spr = hotbarSlotSelected;

                InventorySlot invSlot = slot.GetComponent<InventorySlot>();
                var selected = GetSelectedItem();
                if (selected != null && (selected.category == ItemCategory.Weapons || selected.category == ItemCategory.Tools))
                {
                    if (handHeldManager != null)
                    {
                        handHeldManager.SetHandHeldStats(selected.prefab);
                        handHeldManager.enabled = true;
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

            slot.GetComponent<Image>().sprite = spr;
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
                newItem.transform.SetParent(slotObj.transform.parent.parent.GetChild(1));
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
                newItem.transform.SetParent(slotObj.transform.parent.parent.GetChild(1));
                newItem.transform.localScale = new Vector3(1, 1, 1);
                slot.SetHeldItem(newItem);
            }
        }
    }

    public List<ItemTypeAndAmount> GetAllItems()
    {
        items.Clear();

        AccumulateItemsFromSlots(slots);
        AccumulateItemsFromSlots(hotbarSlots);
        return items;
    }

    public List<ItemTypeAndAmount> GetInventoryItemsOnly()
    {
        items.Clear();

        AccumulateItemsFromSlots(slots);
        return items;
    }

    private void AccumulateItemsFromSlots(GameObject[] slotArray)
    {
        if (slotArray == null) return;

        foreach (GameObject slot in slotArray)
        {
            if (slot == null) continue;

            InventorySlot slotScript = slot.GetComponent<InventorySlot>();

            if (slotScript != null && slotScript.heldItem != null)
            {
                InventoryItem itemScript = slotScript.heldItem.GetComponent<InventoryItem>();
                if (itemScript == null || itemScript.itemScriptableObject == null || itemScript.stackCurrent <= 0)
                {
                    continue;
                }

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

    // Compatibility methods used by other systems
    public bool HasItem(ItemSO item, int requiredAmount)
    {
        var all = GetAllItems();
        foreach (var it in all)
        {
            if (it.item == item)
            {
                if (requiredAmount <= 0)
                {
                    return it.amount > 0;
                }
                return it.amount >= requiredAmount;
            }
        }
        return false;
    }

    private int TopUpExistingStacksInSlots(GameObject[] slotArray, ItemSO item, int remaining)
    {
        if (slotArray == null || item == null || remaining <= 0) return remaining;

        for (int i = 0; i < slotArray.Length && remaining > 0; i++)
        {
            var slotObj = slotArray[i];
            if (slotObj == null) continue;

            var slot = slotObj.GetComponent<InventorySlot>();
            if (slot == null || slot.heldItem == null) continue;

            var invItem = slot.heldItem.GetComponent<InventoryItem>();
            if (invItem == null || invItem.itemScriptableObject != item) continue;

            int maxStack = item.maxStackSize > 0 ? item.maxStackSize : invItem.stackMax;
            if (maxStack <= 1 || invItem.stackCurrent >= maxStack) continue;

            int space = maxStack - invItem.stackCurrent;
            int toMove = Mathf.Min(space, remaining);
            invItem.stackCurrent += toMove;
            remaining -= toMove;
        }

        return remaining;
    }

    private int TopUpExistingStacks(ItemSO item, int remaining)
    {
        remaining = TopUpExistingStacksInSlots(slots, item, remaining);
        remaining = TopUpExistingStacksInSlots(hotbarSlots, item, remaining);
        return remaining;
    }

    public void AddItem(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return;

        int remaining = amount;

        // Top up existing stacks first (main inventory + hotbar).
        remaining = TopUpExistingStacks(item, remaining);

        // Then create new stacks in empty slots
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            var slotObj = slots[i];
            if (slotObj == null) continue;

            var slot = slotObj.GetComponent<InventorySlot>();
            if (slot == null) continue;

            if (slot.heldItem == null)
            {
                if (itemPrefab == null) break;

                Transform container = slotObj.transform.parent.parent.GetChild(1);
                GameObject newItem = Instantiate(itemPrefab, container);
                var invItem = newItem.GetComponent<InventoryItem>();
                if (invItem == null) continue;
                invItem.itemScriptableObject = item;
                int maxStack = item.maxStackSize > 0 ? item.maxStackSize : invItem.stackMax;
                int put = Mathf.Min(remaining, maxStack);
                invItem.stackCurrent = put;
                newItem.transform.position = slot.transform.position;
                newItem.transform.localScale = Vector3.one;
                slot.SetHeldItem(newItem);
                remaining -= put;
            }
        }

        HotBarItemChanged();
    }

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

    public void UseThisItem(InventorySlot slot)
    {
        if (hotbarSlots == null || hotbarSlots.Length == 0)
            return;

        if (selectedHotbarIndex < 0 || selectedHotbarIndex >= hotbarSlots.Length)
            return;

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
            isStoreOpened = false;
        }
    }

    public void OpenAndCloseStore()
    {
        if (isStoreOpened)
        {
            isStoreOpened = false;
        }
        else
        {
            CloseSidebar();
            isStoreOpened = true;
        }
    }

    public void OpenAndCloseDebt()
    {
        if (isDebtOpened)
        {
            isDebtOpened = false;
        }
        else
        {
            CloseSidebar();
            isDebtOpened = true;
        }
    }

    public void OpenAndCloseMilestone()
    {
        if (isMilestoneOpened)
        {
            isMilestoneOpened = false;
        }
        else
        {
            CloseSidebar();
            isMilestoneOpened = true;
        }
    }

    public void CloseSidebar()
    {
        if (isCraftingOpened)
        {
            isCraftingOpened = false;
        }
        if (isStoreOpened)
        {
            isStoreOpened = false;
        }
        if (isDebtOpened)
        {
            isDebtOpened = false;
        }
        if (isMilestoneOpened)
        {
            isMilestoneOpened = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //if(GetSelectedItem() && (GetSelectedItem().category == ItemCategory.Weapons))
        //{
        //    handHeldManager.enabled = false;
        //}
        pointerDown = true;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();

            if (slot != null && slot.heldItem != null)
            {
                draggedObject = slot.heldItem;
                slot.heldItem = null;
                lastItemSlot = clickedObject;
                scrollRect.enabled = false;
                draggedObject.transform.SetParent(transform);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        if (draggedObject != null && eventData.pointerCurrentRaycast.gameObject != null && eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();

            //There isnt item in the slot - place item
            if (slot != null && slot.heldItem == null)
            {
                slot.SetHeldItem(draggedObject);
                draggedObject.transform.SetParent(slot.transform.parent.parent.GetChild(1));
            }
            //There is item in the slot - switch items
            else if (slot != null && slot.heldItem != null && slot.heldItem.GetComponent<InventoryItem>().stackCurrent == slot.heldItem.GetComponent<InventoryItem>().stackMax
                || slot != null && slot.heldItem != null && slot.heldItem.GetComponent<InventoryItem>().itemScriptableObject != draggedObject.GetComponent<InventoryItem>().itemScriptableObject)
            {
                lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(slot.heldItem);
                slot.heldItem.transform.SetParent(lastItemSlot.transform.parent.parent.GetChild(1));
                slot.SetHeldItem(draggedObject);
                draggedObject.transform.SetParent(slot.transform.parent.parent.GetChild(1));
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
                    if (lastItemSlot != null)
                    {
                        lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(draggedObject);
                        draggedObject.transform.SetParent(lastItemSlot.transform.parent.parent.GetChild(1));
                    }
                }
            }
            //Return item to last slot
            else if (clickedObject.name != "DropItem")
            {
                if (lastItemSlot != null)
                {
                    lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(draggedObject);
                    // parent it under the original slot's container so it hides with that UI
                    draggedObject.transform.SetParent(lastItemSlot.transform.parent.parent.GetChild(1));
                }
                else if (slot != null)
                {
                    // fallback to clicked slot's container
                    slot.SetHeldItem(draggedObject);
                    draggedObject.transform.SetParent(slot.transform.parent.parent.GetChild(1));
                }
            }
            //Drop item
            else
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Vector3 position = ray.GetPoint(3);

                // Prefer dropping onto the ground while ignoring non-clickable surfaces.
                const int NonClickableLayer = 1;
                int groundMask = LayerMask.GetMask("Ground") & ~(1 << NonClickableLayer);
                if (Physics.Raycast(ray, out RaycastHit groundHit, 1000f, groundMask))
                {
                    position = groundHit.point;
                }

                GameObject newItem = Instantiate(draggedObject.GetComponent<InventoryItem>().itemScriptableObject.prefab, position, new Quaternion());
                newItem.GetComponent<itemPickable>().itemScriptableObject = draggedObject.GetComponent<InventoryItem>().itemScriptableObject;

                lastItemSlot.GetComponent<InventorySlot>().heldItem = null;
                Destroy(draggedObject);
            }

            HotBarItemChanged();
            draggedObject = null;
            scrollRect.enabled = true;
        }
        else if (draggedObject != null && eventData.button == PointerEventData.InputButton.Left)
        {
            lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(draggedObject);
            draggedObject.transform.SetParent(lastItemSlot.transform.parent.parent.GetChild(1));

            HotBarItemChanged();
            draggedObject = null;
            scrollRect.enabled = true;
        }
    }

    public void ItemPicked(GameObject pickedItem)
    {
        if (pickedItem == null) return;

        var pickable = pickedItem.GetComponent<itemPickable>();
        if (pickable == null || pickable.itemScriptableObject == null) return;

        ItemSO pickedSO = pickable.itemScriptableObject;
        int remaining = 1;

        // First try to top up existing stacks (main inventory + hotbar).
        remaining = TopUpExistingStacks(pickedSO, remaining);

        // Then create new stack(s) in empty slots if needed
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            InventorySlot slot = slots[i].GetComponent<InventorySlot>();
            if (slot == null) continue;

            if (slot.heldItem == null)
            {
                if (itemPrefab == null) break;

                GameObject newItem = Instantiate(itemPrefab);
                var invItem = newItem.GetComponent<InventoryItem>();
                if (invItem == null) continue;
                invItem.itemScriptableObject = pickedSO;
                int put = Mathf.Min(remaining, pickedSO.maxStackSize);
                invItem.stackCurrent = put;
                newItem.transform.SetParent(slot.transform.parent.parent.GetChild(1));
                newItem.transform.localScale = new Vector3(1, 1, 1);
                slot.SetHeldItem(newItem);
                remaining -= put;
            }
        }

        // If we were able to collect the item, destroy the world object
        if (remaining == 0)
        {
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

    public InventorySlot GetSelectedItemSlot()
    {
        if (hotbarSlots == null || hotbarSlots.Length == 0)
            return null;

        if (selectedHotbarIndex < 0 || selectedHotbarIndex >= hotbarSlots.Length)
            return null;

        InventorySlot slot = hotbarSlots[selectedHotbarIndex].GetComponent<InventorySlot>();
        if (slot == null || slot.heldItem == null)
            return null;

        return slot;
    }

    public InventorySlot GetDraggedItemSlot()
    {
        if (draggedObject != null && lastItemSlot != null)
        {
            return lastItemSlot.GetComponent<InventorySlot>();
        }

        return null;
    }

    public ItemSO GetDraggedItem()
    {
        return draggedObject.GetComponent<InventoryItem>().itemScriptableObject;
    }
    public void CraftItem(List<ItemTypeAndAmount> itemsToCraft, List<ItemTypeAndAmount> itemsToDestroy)
    {
        foreach (ItemTypeAndAmount itemToCraft in itemsToCraft)
        {
            AddItem(itemToCraft.item, itemToCraft.amount);
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

    // Sell specified items from main inventory slots only (do not touch hotbar).
    public void SellItems(List<ItemTypeAndAmount> itemsToSell)
    {
        if (itemsToSell == null || itemsToSell.Count == 0) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetComponent<InventorySlot>().heldItem != null)
            {
                InventorySlot slotScript = slots[i].GetComponent<InventorySlot>();
                InventoryItem itemScript = slotScript.heldItem.GetComponent<InventoryItem>();

                foreach (ItemTypeAndAmount itemToSell in itemsToSell)
                {
                    if (itemToSell.item == itemScript.itemScriptableObject)
                    {
                        if (itemToSell.amount >= itemScript.stackCurrent)
                        {
                            slotScript.heldItem = null;
                            Destroy(itemScript.gameObject);
                        }
                        else if (itemToSell.amount < itemScript.stackCurrent)
                        {
                            itemScript.stackCurrent -= itemToSell.amount;
                        }
                    }
                }
            }
        }
    }
}