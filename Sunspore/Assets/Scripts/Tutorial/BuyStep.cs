using System.Collections.Generic;
using Sunspore;
using UnityEngine;

public class BuyStep : TutorialStep
{
    [SerializeField]
    InventoryManager inventoryManager;

    [SerializeField]
    List<ItemSO> buyTargets;

    [SerializeField]
    List<int> buyAmounts;

    private List<int> _initialAmounts = new();

    public override void Begin()
    {
        _initialAmounts.Clear();

        if (buyTargets != null)
        {
            foreach (var target in buyTargets)
            {
                _initialAmounts.Add(GetCurrentAmount(target));
            }
        }

        Debug.Log("Dialog: Use the store to buy the plants you need.");
    }

    public override void Check()
    {
        if (buyTargets == null || buyAmounts == null)
            return;
        if (buyTargets.Count != buyAmounts.Count || buyTargets.Count != _initialAmounts.Count)
            return;

        for (int i = 0; i < buyTargets.Count; i++)
        {
            int currentAmount = GetCurrentAmount(buyTargets[i]);
            int gained = currentAmount - _initialAmounts[i];

            if (gained < buyAmounts[i])
            {
                return;
            }
        }

        TutorialManager.Instance.CompleteStep();
    }

    public override void End() { }

    private int GetCurrentAmount(ItemSO target)
    {
        if (inventoryManager == null || target == null)
        {
            return 0;
        }

        List<ItemTypeAndAmount> allItems = inventoryManager.GetAllItems();
        foreach (var entry in allItems)
        {
            if (entry.item == target)
            {
                return entry.amount;
            }
        }

        return 0;
    }
}
