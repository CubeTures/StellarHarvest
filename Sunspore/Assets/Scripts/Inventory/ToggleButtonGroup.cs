using System;
using Sunspore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

class ToggleButtonGroup : MonoBehaviour
{
	[SerializeField] ToggleButton[] toggles;


	void Awake()
	{
		int selected = 0;
		Button[] buttons = GetComponentsInChildren<Button>(true);
		for (int i = 0; i < toggles.Length; i++)
		{
			int capture = i;
			buttons[i].onClick.AddListener(() => Intercept(capture));
			toggles[i].img = buttons[i].GetComponent<Image>();
			selected = toggles[i].selected ? i : selected;

			if (!string.IsNullOrEmpty(toggles[i].tooltipText))
			{
				var trigger = buttons[i].GetComponent<TooltipTrigger>();
				if (trigger == null)
					trigger = buttons[i].gameObject.AddComponent<TooltipTrigger>();
				trigger.SetContent(toggles[i].tooltipText);
			}
		}
		Toggle(selected);
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
		for (int k = 0; k < toggles.Length; k++)
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
		int defaultSelected = 0;
		for (int i = 0; i < toggles.Length; i++)
		{
			defaultSelected = toggles[i].defaultSelected ? i : defaultSelected;
			SetStyles(i, false);
		}
		SetStyles(defaultSelected, true);
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