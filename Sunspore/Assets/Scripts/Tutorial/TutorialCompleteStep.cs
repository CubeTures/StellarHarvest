using Sunspore;
using UnityEngine;

public class TutorialCompleteStep : TutorialStep
{
    public override void Begin()
    {
        Debug.Log(
            "Tutorial complete: the full game is now unlocked. You can buy any available item and the day-night cycle will continue."
        );
        TutorialManager.Instance.CompleteStep();
    }

    public override void End()
    {
        // Optionally disable the TutorialManager or mark tutorial as finished in PlayerPrefs.
    }
}
