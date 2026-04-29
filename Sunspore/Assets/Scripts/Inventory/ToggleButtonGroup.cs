using System;
using Sunspore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

class ToggleButtonGroup : MonoBehaviour
{
	[SerializeField] ToggleButton[] toggles;
	[SerializeField]
	[Tooltip("Optional. Same size as Toggles, in the same order. Use when tabs are not all UnityEngine.UI.Button, or are not all under this object — auto-scan only finds child Buttons.")]
	Button[] buttonReferences;
	/// <summary>Entries in <see cref="toggles"/> that were actually wired to a child <see cref="Button"/>.</summary>
	int _pairedCount;


	void Awake()
	{
		if (toggles == null || toggles.Length == 0)
			return;

		Button[] buttons = ResolveButtons();
		if (buttons == null || buttons.Length == 0)
		{
			// Debug.LogWarning("ToggleButtonGroup: no Button components available. Assign buttonReferences (same size as toggles) or add child Button components on " + name + ".");
			return;
		}

		_pairedCount = Mathf.Min(toggles.Length, buttons.Length);
		if (_pairedCount < toggles.Length)
		{
			// Debug.LogWarning(
				// "ToggleButtonGroup: more toggles than buttons on "
					// + name
					// + ". Either shrink the Toggles list, or assign Button References (one per tab, same order).");
		}

		int selected = 0;
		for (int i = 0; i < _pairedCount; i++)
		{
			int capture = i;
			buttons[i].onClick.AddListener(() => Intercept(capture));
			toggles[i].img = buttons[i].GetComponent<Image>();
			if (toggles[i].defaultSelected)
				selected = i;

			if (!string.IsNullOrEmpty(toggles[i].tooltipText))
			{
				var trigger = buttons[i].GetComponent<TooltipTrigger>();
				if (trigger == null)
					trigger = buttons[i].gameObject.AddComponent<TooltipTrigger>();
				trigger.SetContent(toggles[i].tooltipText);
			}
		}

		if (_pairedCount > 0)
			Toggle(Mathf.Clamp(selected, 0, _pairedCount - 1));
	}

	Button[] ResolveButtons()
	{
		if (buttonReferences != null && buttonReferences.Length == toggles.Length)
		{
			for (int i = 0; i < buttonReferences.Length; i++)
			{
				if (buttonReferences[i] == null)
				{
					// Debug.LogError("ToggleButtonGroup on " + name + ": buttonReferences[" + i + "] is null. Assign all entries.");
					return null;
				}
			}
			return buttonReferences;
		}
		if (buttonReferences != null && buttonReferences.Length > 0)
		{
			// Debug.LogWarning(
				// "ToggleButtonGroup on "
					// + name
					// + ": buttonReferences length ("
					// + buttonReferences.Length
					// + ") must match toggles length ("
					// + toggles.Length
					// + "). Clearing buttonReferences and scanning children — prefer fixing the assignment.");
		}
		return GetComponentsInChildren<Button>(true);
	}

	void Intercept(int i)
	{
		ToggleButton t = toggles[i];
		if (!t.selected)
		{
			t.onClick?.Invoke();
			Toggle(i);
		}
	}

	void Toggle(int i)
	{
		for (int k = 0; k < _pairedCount; k++)
		{
			SetStyles(k, false);
		}
		SetStyles(i, true);
	}

	void SetStyles(int i, bool selected)
	{
		ToggleButton t = toggles[i];
		if (t == null) return;

		if (t.img != null)
			t.img.sprite = selected ? t.selectedSprite : t.unselectedSprite;
		if (t.selectedContent != null && t.unselectedContent != null)
		{
			t.selectedContent.SetActive(selected);
			t.unselectedContent.SetActive(!selected);
		}
		t.selected = selected;
	}

	public void Reset()
	{
		if (toggles == null || _pairedCount == 0)
			return;
		int defaultSelected = 0;
		for (int i = 0; i < _pairedCount; i++)
		{
			if (toggles[i].defaultSelected)
				defaultSelected = i;
			SetStyles(i, false);
		}
		SetStyles(Mathf.Clamp(defaultSelected, 0, _pairedCount - 1), true);
	}
}

[Serializable]
class ToggleButton
{
	public Sprite unselectedSprite;
	public Sprite selectedSprite;
	public GameObject unselectedContent;
	public GameObject selectedContent;
	public UnityEvent onClick;
	public bool defaultSelected;
	public string tooltipText;
	[HideInInspector] public Image img;
	[HideInInspector] public bool selected;
}