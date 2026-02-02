using System;
using System.Collections.Generic;
using UnityEngine;

public class Hotbar : MonoBehaviour
{
    [SerializeField] GameObject hotbarSlotPrefab;

    private const int NUM_SLOTS = 8;
    private List<HotbarSlot> slots = new();
    private int selected = 0;

    void Awake()
    {
        for (int i = 0; i < NUM_SLOTS; i++)
        {
            GameObject inst = Instantiate(hotbarSlotPrefab, transform);
            HotbarSlot slot = inst.GetComponent<HotbarSlot>();
            slots.Add(slot);
        }
    }

    public void ChangeSelected(float scroll)
    {
        if (scroll < 0)
        {
            selected += 1;
            if (selected >= slots.Count)
            {
                selected = 0;
            }
        }
        else if (scroll > 0)
        {
            selected -= 1;
            if (selected < 0)
            {
                selected = slots.Count - 1;
            }
        }
    }

    public InventoryItem GetSelectedItem(Inventory inventory)
    {
        return inventory.GetIndex(selected)?.item;
    }

    public void UpdateUI(Inventory inventory)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].UpdateUI(inventory.GetIndex(i), selected == i);
        }
    }
}