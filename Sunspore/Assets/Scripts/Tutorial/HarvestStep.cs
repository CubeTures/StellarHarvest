using System.Collections.Generic;
using Sunspore;
using UnityEngine;

public class HarvestStep : TutorialStep
{
    [SerializeField]
    InventoryManager inventoryManager;

    [SerializeField]
    ItemSO harvestTarget;

    [SerializeField]
    int harvestAmount = 1;

    private int _initialAmount;

    public override void Begin()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("HarvestStep: inventoryManager is not assigned.");
        }

        if (harvestTarget == null)
        {
            Debug.LogWarning("HarvestStep: harvestTarget is not assigned.");
        }

        _initialAmount = GetCurrentAmount();
        Debug.Log("Harvest native plants");
    }

    public override void Check()
    {
        int currentAmount = GetCurrentAmount();

        if (currentAmount >= _initialAmount + harvestAmount)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        // optional: tweak tutorial plants or visuals after harvest
    }

    private int GetCurrentAmount()
    {
        if (inventoryManager == null || harvestTarget == null)
        {
            return 0;
        }

        List<ItemTypeAndAmount> allItems = inventoryManager.GetAllItems();
        foreach (var entry in allItems)
        {
            if (IsSameItem(entry.item, harvestTarget))
            {
                return entry.amount;
            }
        }

        return 0;
    }

    private bool IsSameItem(ItemSO a, ItemSO b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        // Prefer direct ScriptableObject reference, but allow display-name fallback
        // for cases where equivalent items come from different SO instances.
        if (a == b)
        {
            return true;
        }

        return string.Equals(
            a.displayName?.Trim(),
            b.displayName?.Trim(),
            System.StringComparison.OrdinalIgnoreCase
        );
    }
}
