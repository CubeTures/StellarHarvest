using Sunspore;
using UnityEngine;

public class HoldToolStep : TutorialStep
{
	[SerializeField] int requiredSlotIndex = 2;

	public override void Begin()
	{
	}

	public override void Check()
	{
		if (InventoryManager.Instance.SelectedHotbarIndex == requiredSlotIndex)
		{
			TutorialManager.Instance.CompleteStep();
		}
	}

	public override void End() { }
}
