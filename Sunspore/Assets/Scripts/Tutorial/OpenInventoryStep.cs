using UnityEngine;

public class OpenInventoryStep : TutorialStep
{
    private bool _openedOnce;

    public override void Begin()
    {
        _openedOnce = false;
        PlayerPrefManager.SetInventoryOpenCount(0);
        Debug.Log("Dialog: Press E to open your inventory, then press E again to close it.");
    }

    public override void Check()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }

        if (!_openedOnce && InventoryManager.Instance.isInventoryOpened)
        {
            _openedOnce = true;
            return;
        }

        if (_openedOnce && !InventoryManager.Instance.isInventoryOpened)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        // inventory has been opened at least once; continue tutorial
    }
}
