using UnityEngine;

public class SignalStep : TutorialStep
{
    [SerializeField] TutorialManager.Actions signal;

    public TutorialManager.Actions ExpectedAction => signal;

    public override void Begin()
    {
        // If the player already planted a seed (e.g. right after tilling) while a non-PLANT
        // signal step was active, the PLANT broadcast was missed — finish this step on entry.
        if (signal == TutorialManager.Actions.PLANT && TutorialManager.Instance.ConsumePendingPlantForPlantStep())
        {
            TutorialManager.Instance.CompleteStep();
            return;
        }

        TutorialManager.Instance.SubscribeBroadcast(OnSignal);
    }

    public void OnSignal(TutorialManager.Actions action)
    {
        if (action == signal)
        {
            TutorialManager.Instance.CompleteStep();
        }
    }

    public override void End()
    {
        TutorialManager.Instance.UnsubscribeBroadcast(OnSignal);
    }
}
