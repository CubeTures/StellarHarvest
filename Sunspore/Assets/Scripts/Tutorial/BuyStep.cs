using System.Collections.Generic;
using UnityEngine;

public class BuyStep : TutorialStep
{
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] List<ItemSO> buyTargets;
    [SerializeField] List<int> buyAmounts;

    public override void Begin()
    {
        // only buy the necessary plants for crafting next step
        // crafting step will award player extra money or plants after
        Debug.Log("Dialog: Buy those plants!");
    }

    public override void Check()
    {
        bool clear = true;

        for (int i = 0; i < buyTargets.Count; i++)
        {
            clear = clear && inventoryManager.HasItem(buyTargets[i], buyAmounts[i]);
        }

        if (clear)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {

    }
}