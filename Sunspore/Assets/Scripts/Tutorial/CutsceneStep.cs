using Sunspore;
using UnityEngine;

public class CutsceneStep : TutorialStep
{
    [SerializeField]
    private bool autoCompleteOnBegin = true;

    public override void Begin()
    {
        if (autoCompleteOnBegin)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End() { }
}
