using UnityEngine;

public class HoldToolStep : TutorialStep
{
	public override void Begin()
	{
	}

	public override void Check()
	{
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			TutorialManager.Instance.CompleteStep();
		}
	}

	public override void End() { }
}
