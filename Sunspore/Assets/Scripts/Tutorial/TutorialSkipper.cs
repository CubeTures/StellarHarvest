using System;
using Sunspore;
using UnityEngine;

class TutorialSkipper : MonoBehaviour
{
	[SerializeField] Dialogue dl;
	[SerializeField] TutorialManager tm;
	[SerializeField] DebtManager dm;
	[SerializeField] InventoryManager im;
	[SerializeField] ItemSO glowbudSeed;
	[SerializeField] GameObject visual;

	bool visible = false;

	void Update()
	{
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.P))
		{
			visible = true;
		}
		visual.SetActive(tm.IsTutorialActive && visible);
	}

	public void SkipTutorial()
	{
		dm.SkipFirstDebt();
		dl.CompleteDialog();
		tm.CompleteTutorial();
		im.AddItem(glowbudSeed, 1);
	}
}