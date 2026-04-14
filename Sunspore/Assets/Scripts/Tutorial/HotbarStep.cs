using UnityEngine;

class HotbarStep : TutorialStep
{
	[SerializeField] ItemSO item;

	public override void Begin()
	{
		// throw new System.NotImplementedException();
	}

	public override void Check()
	{
		if (InventoryManager.Instance.HasHotbarItem(item))
		{
			TutorialManager.Instance.CompleteStep();
		}
	}

	public override void End()
	{
		// throw new System.NotImplementedException();
	}
}