using System.Collections.Generic;
using Sunspore;
using UnityEngine;

public class SellStep : TutorialStep
{
    [SerializeField]
    InventoryManager inventoryManager;

    [SerializeField]
    ItemSO sellTarget;

    [SerializeField]
    int sellAmount = 1;

    private int _initialAmount;

    public override void Begin()
    {
        _initialAmount = GetCurrentAmount();
        Debug.Log(
            "Dialog: Open the store inside the inventory tab and sell some of your harvested plants."
        );
    }

    public override void Check()
    {
        int currentAmount = GetCurrentAmount();

        if (currentAmount <= _initialAmount - sellAmount)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        // optional: reward player or unlock next store options
    }

    private int GetCurrentAmount()
    {
        if (inventoryManager == null || sellTarget == null)
        {
            return 0;
        }

        List<ItemTypeAndAmount> allItems = inventoryManager.GetAllItems();
        foreach (var entry in allItems)
        {
            if (entry.item == sellTarget)
            {
                return entry.amount;
            }
        }

        return 0;
    }
}
