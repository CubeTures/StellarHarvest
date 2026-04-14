using UnityEngine;

public class OpenInventoryStep : TutorialStep
{
    [SerializeField] private int requiredOpenCount = 1;

    public override void Begin()
    {
        PlayerPrefManager.SetInventoryOpenCount(0);
        Debug.Log("Dialog: Press E to open your inventory and toolbelt.");
    }

    public override void Check()
    {
        if (PlayerPrefManager.GetInventoryOpenCount() >= requiredOpenCount)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        // inventory has been opened at least once; continue tutorial
    }
}
