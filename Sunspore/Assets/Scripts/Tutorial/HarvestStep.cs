using UnityEngine;

public class HarvestStep : TutorialStep
{
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] ItemSO harvestTarget;
    [SerializeField] int harvestAmount;

    public override void Begin()
    {
        Debug.Log("Dialog: Harvest those plants!");
    }

    public override void Check()
    {
        if (inventoryManager.HasItem(harvestTarget, harvestAmount))
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        // change attributes of tutorial plants or smth
    }
}