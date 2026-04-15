using UnityEngine;

public class SignalStep : TutorialStep
{
	[SerializeField] TutorialManager.Actions signal;

	public override void Begin()
	{
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
