using Sunspore;
using UnityEngine;

public class HoldToolStep : TutorialStep
{
	[SerializeField] int requiredSlotIndex = 2;

	public override void Begin()
	{
		TutorialManager.Instance.availableActions[TutorialManager.Actions.INVENTORY] = false;
		TutorialManager.Instance.availableActions[TutorialManager.Actions.HOTBAR] = true;
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
