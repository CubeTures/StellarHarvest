using System;
using System.Collections.Generic;
using System.Linq;
using Sunspore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

class DebtManager : MonoBehaviour
{
	public static DebtManager Instance { get; private set; }

	[SerializeField] DayNightManager dayNightManager;
	[SerializeField] Wallet wallet;

	[SerializeField] int[] payments;
	[SerializeField] TMP_Text currentLabels, currentPrices, totalPrice, balanceLabel, balancePrice;
	[SerializeField] PaymentTimeline currentTimeline;
	[SerializeField] Transform upcomingParent;
	[SerializeField] GameObject upcomingPrefab;
	[SerializeField] TMP_Text defaultWarning;
	Color red = new(255 / 225f, 114 / 225f, 116 / 225f), blue = new(172 / 225f, 207 / 225f, 255 / 225f);

	List<Item> receipt = new();
	int currentPayment = 0;
	int missedPayments = 0;
	int completedInstallments = 0;
	int lastNotification = 0;

	class Item
	{
		public string label;
		public int price;
	}

	bool _initialized;

	void Awake()
	{
		Instance = this;
	}

	public int RemainingPayments => _initialized
		? upcomingParent.childCount + (OwedAmount() > 0 ? 1 : 0)
		: payments.Length;

	/// <summary>True after the first successful <see cref="MakePayment"/> (used for tutorial DEBT step).</summary>
	public bool HasCompletedFirstInstallment => completedInstallments >= 1;

	/// <summary>Whole days until the current payment is due (same basis as <see cref="PaymentTimeline.DaysLeft"/>).</summary>
	public int GetDaysLeftUntilNextPayment()
	{
		if (currentTimeline == null) return 0;
		return Mathf.Max(0, currentTimeline.DaysLeft());
	}

	void Start()
	{
		_initialized = true;
		receipt.Add(new Item
		{
			label = "Current Payment",
			price = payments[currentPayment++],
		});

		currentTimeline.SetDayNightManager(dayNightManager);
		UnityEvent unityEvent = new();
		unityEvent.AddListener(MakePayment);
		currentTimeline.SetOnClick(unityEvent);

		currentTimeline.SetAmount(receipt[0].price);
		currentTimeline.SetDueDate(0.001f);
		currentTimeline.SetCanPay(true);

		for (int i = 1; i < payments.Length; i++)
		{
			GameObject upcoming = Instantiate(upcomingPrefab, upcomingParent);
			PaymentTimeline timeline = upcoming.GetComponent<PaymentTimeline>();
			timeline.SetDayNightManager(dayNightManager);
			timeline.SetDueDate(2 * i * dayNightManager.dayLengthSeconds);
			timeline.SetAmount(payments[i]);
		}

		UpdateText();
		UpdateMe.Instance.Subscribe(_Update);
	}

	void _Update()
	{
		CheckExpiration();
		UpdatePayable();
		SetNotification();
	}

	void CheckExpiration()
	{
		if (currentTimeline.IsDue())
		{
			Item last = receipt[receipt.Count - 1];

			if (last.label == "Interest")
			{
				last = receipt[receipt.Count - 2];
			}
			else
			{
				receipt.RemoveAt(receipt.Count - 1);
				receipt.Add(new Item
				{
					label = "Missed Payment",
					price = last.price,
				});
			}

			receipt.Add(new Item
			{
				label = "Interest",
				price = (int)Mathf.Round(last.price * (missedPayments == 0 ? .15f : .30f)),
			});

			missedPayments += 1;
			if (missedPayments == 3)
			{
				GameManager.gm.GameLose(wallet.GetAmount() - OwedAmount() - RemainingAmount());
			}

			AddNextPayment();
		}
	}

	void UpdatePayable()
	{
		int diff = wallet.GetAmount() - OwedAmount();
		bool tutorialBlocksFurtherPayments = TutorialManager.Instance != null
			&& TutorialManager.Instance.IsTutorialActive
			&& completedInstallments >= 1;

		if (diff >= 0 && !tutorialBlocksFurtherPayments)
		{
			currentTimeline.SetCanPay(true);
			balanceLabel.color = blue;
			balancePrice.color = blue;
		}
		else
		{
			currentTimeline.SetCanPay(false);
			balanceLabel.color = red;
			balancePrice.color = red;
		}

		balancePrice.text = diff.ToString("N0") + " credits";
	}

	void SetNotification()
	{
		if (!TutorialManager.Instance.IsTutorialActive)
		{
			int daysLeft = currentTimeline.DaysLeft();
			if (daysLeft != lastNotification)
			{
				NotificationManager.Instance.PopFixedNotification();

				string daysText = $"in {daysLeft} day{(daysLeft == 1 ? "" : "s")}";
				if (daysLeft == 0)
				{
					daysText = "today";
				}
				NotificationManager.Instance.PushFixedNotification($"Pay off your debt {daysText}.");
			}
		}
	}

	int OwedAmount()
	{
		return receipt.Aggregate(0, (acc, x) => acc + x.price);
	}

	int RemainingAmount()
	{
		if (currentPayment == payments.Length)
		{
			return 0;
		}

		return payments[currentPayment..].Aggregate(0, (acc, x) => acc + x);
	}

	public void MakePayment()
	{
		if (TutorialManager.Instance != null
			&& TutorialManager.Instance.IsTutorialActive
			&& completedInstallments >= 1)
		{
			return;
		}

		if (wallet.GetAmount() >= OwedAmount())
		{
			TutorialManager.Instance.BroadcastSignal(TutorialManager.Actions.DEBT);
			wallet.Subtract(OwedAmount());
			receipt.Clear();
			completedInstallments++;
			AddNextPayment();
			UpdateText();
		}
	}

	void AddNextPayment()
	{
		if (upcomingParent.childCount == 0)
		{
			if (OwedAmount() == 0)
			{
				GameManager.gm.GameWin();
			}

			// missed last payment; keep going
			currentTimeline.SetAmount(OwedAmount());
			currentTimeline.SetDueDate(2 * dayNightManager.dayLengthSeconds);
		}
		else
		{
			Transform child = upcomingParent.GetChild(0);
			PaymentTimeline timeline = child.GetComponent<PaymentTimeline>();

			receipt.Add(new Item
			{
				label = "Current Payment",
				price = timeline.GetAmount(),
			});

			currentTimeline.SetAmount(OwedAmount());
			currentTimeline.SetDueDate(timeline.GetDueTime(), false);
			Destroy(child.gameObject);
		}

		UpdateText();
	}

	void UpdateText()
	{
		currentLabels.text = "";
		currentPrices.text = "";
		foreach (Item i in receipt)
		{
			if (currentLabels.text.Length > 0)
			{
				currentLabels.text += "\n";
				currentPrices.text += "\n";
			}
			currentLabels.text += i.label;
			currentPrices.text += i.price.ToString("N0") + " credits";
		}

		totalPrice.text = OwedAmount().ToString("N0") + " credits";

		if (missedPayments == 2)
		{
			defaultWarning.text = "Next Missed Payment Defaults";
			defaultWarning.color = red;
		}
		else
		{
			defaultWarning.text = 2 - missedPayments + " Missed Payment Until Default";
		}
	}

	public void SkipFirstDebt()
	{
		wallet.Add(50);
		MakePayment();
	}
}