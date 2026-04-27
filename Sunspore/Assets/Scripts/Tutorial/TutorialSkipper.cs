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

	bool skipped = false;

	void Update()
	{
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.P))
		{
			SkipTutorial();
		}
	}

	public void SkipTutorial()
	{
		if (skipped) return;
		skipped = true;
		dl.CompleteDialog();
		NotificationManager.Instance.PopFixedNotification();
		tm.CompleteTutorial();
		im.AddItem(glowbudSeed, 1);
	}
}