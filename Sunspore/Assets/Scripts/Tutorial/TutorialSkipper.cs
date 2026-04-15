using System;
using Sunspore;
using UnityEngine;

class TutorialSkipper: MonoBehaviour
{
	[SerializeField] Dialogue dl;
	[SerializeField] TutorialManager tm;
	[SerializeField] DebtManager dm;
	[SerializeField] InventoryManager im;
	[SerializeField] ItemSO glowbudSeed;

	void Update()
	{
		gameObject.SetActive(tm.IsTutorialActive);
	}

	public void SkipTutorial()
	{
		dm.SkipFirstDebt();
		dl.CompleteDialog();
		tm.CompleteTutorial();
		im.AddItem(glowbudSeed, 1);
	}
}