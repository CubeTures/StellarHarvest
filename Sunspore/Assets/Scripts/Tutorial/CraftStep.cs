using System.Collections.Generic;
using Sunspore;
using UnityEngine;

public class CraftStep : TutorialStep
{
    [SerializeField]
    InventoryManager inventoryManager;

    [SerializeField]
    ItemSO craftTarget;

    [SerializeField]
    int craftAmount = 1;

    private int _initialAmount;

    public override void Begin()
    {
        _initialAmount = GetCurrentAmount();
        Debug.Log("Dialog: Craft a sword using some of your harvested plants.");
    }

    public override void Check()
    {
        int currentAmount = GetCurrentAmount();

        if (currentAmount >= _initialAmount + craftAmount)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        // award player money or plants for completing the tutorial if desired
    }

    private int GetCurrentAmount()
    {
        if (inventoryManager == null || craftTarget == null)
        {
            return 0;
        }

        List<ItemTypeAndAmount> allItems = inventoryManager.GetAllItems();
        foreach (var entry in allItems)
        {
            if (entry.item == craftTarget)
            {
                return entry.amount;
            }
        }

        return 0;
    }
}
