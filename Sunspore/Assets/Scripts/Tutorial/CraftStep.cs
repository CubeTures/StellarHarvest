using UnityEngine;

public class CraftStep : TutorialStep
{
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] ItemSO craftTarget;
    [SerializeField] int craftAmount;

    public override void Begin()
    {
        Debug.Log("Dialog: Craft that item!");
    }

    public override void Check()
    {
        if (inventoryManager.HasItem(craftTarget, 0))
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        // award player money or plants for completing the tutorial
    }
}