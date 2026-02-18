using UnityEngine;

public class CutsceneStep : TutorialStep
{
    public override void Begin()
    {
        Debug.Log("Cutscene: Welcome to the Galaxy, Farmer!");
        TutorialManager.Instance.CompleteStep();
    }

    public override void End()
    {
        // end opening cutscene
        // play some animation getting out of house?
    }
}