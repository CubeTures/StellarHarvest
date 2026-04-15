using UnityEngine;

public class MoveStep : TutorialStep
{
    [SerializeField] private int requiredMoveCount = 1;

    public override void Begin()
    {
        PlayerPrefManager.SetMoveCount(0);
        Debug.Log("Dialog: Use WASD to move around.");
    }

    public override void Check()
    {
        if (PlayerPrefManager.GetMoveCount() >= requiredMoveCount)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        // player has moved at least once; continue tutorial
    }
}
