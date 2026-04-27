using Sunspore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

class PaymentTimeline : MonoBehaviour
{
	DayNightManager dayNightManager;
	[SerializeField] TMP_Text timeLabel;
	[SerializeField] RectTransform timelineOuter, timelineInner;
	[SerializeField] Button button;
	[SerializeField] TMP_Text costLabel;
	[SerializeField] Image image;
	[SerializeField] Sprite buttonContainer, staticContainer;
	[SerializeField] float buttonSlice, staticSlice;

	int amount = 0;
	bool canPay = false;
	float startTime, dueTime, gameTime;

	void Start()
	{
		UpdateMe.Instance.Subscribe(_Update);
	}

	public void SetDayNightManager(DayNightManager dayNightManager)
	{
		this.dayNightManager = dayNightManager;
	}

	public void SetOnClick(UnityEvent onClick)
	{
		button.onClick.AddListener(() => onClick.Invoke());
	}

	public void SetAmount(int amount)
	{
		this.amount = amount;
		UpdateText();
	}

	public void SetCanPay(bool canPay)
	{
		this.canPay = canPay;
		button.enabled = canPay;
		image.sprite = canPay ? buttonContainer : staticContainer;
		image.pixelsPerUnitMultiplier = canPay ? buttonSlice : staticSlice;
		UpdateText();
	}

	void UpdateText()
	{
		costLabel.text = (canPay ? "Pay " : "") + amount.ToString("N0");
	}

	public void SetDueDate(float time, bool setStart = true)
	{
		startTime = setStart ? 0 : startTime;
		dueTime = time;
	}

	void Update()
	{
		float remaining = (dueTime - gameTime) / (dueTime - startTime);
		timelineInner.sizeDelta = new(timelineOuter.sizeDelta.x * (1 - remaining), timelineInner.sizeDelta.y);

		string label = "Due ";
		int days = (int)((dueTime - gameTime) / dayNightManager.dayLengthSeconds);
		if (days == 0)
		{
			label += "Today";
		}
		else if (days == 1)
		{
			label += "in 1 Day";
		}
		else
		{
			label += "in " + days + " Days";
		}
		timeLabel.text = label;
	}

	void _Update()
	{
		if (!dayNightManager.timeStopped)
		{
			gameTime += Time.deltaTime;
		}
	}

	public bool IsDue()
	{
		return gameTime >= dueTime;
	}

	public float GetDueTime()
	{
		return dueTime;
	}

	public int DaysLeft()
	{
		return (int)((dueTime - gameTime) / dayNightManager.dayLengthSeconds);
	}

	public int GetAmount()
	{
		return amount;
	}

	void Oestroy()
	{
		UpdateMe.Instance.Unsubscribe(_Update);		
	}
}