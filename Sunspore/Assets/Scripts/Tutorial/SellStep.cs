using UnityEngine;

public class SellStep : TutorialStep
{
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] ItemSO sellTarget;
    [SerializeField] int sellAmount;

    public override void Begin()
    {
        Debug.Log("Dialog: Sell those plants!");
    }

    public override void Check()
    {
        if (inventoryManager.HasItem(sellTarget, 0))
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        // change attributes of tutorial plants or smth
    }
}