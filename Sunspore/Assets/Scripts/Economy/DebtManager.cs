using System;
using System.Collections.Generic;
using System.Linq;
using Sunspore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

class DebtManager : MonoBehaviour
{
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
	float startTime = 0;

	class Item
	{
		public string label;
		public int price;
	}

	void Start()
	{
		receipt.Add(new Item
		{
			label = "Current Payment",
			price = payments[currentPayment++],
		});

		currentTimeline.SetDayNightManager(dayNightManager);
		UnityEvent unityEvent = new();
		unityEvent.AddListener(MakePayment);
		currentTimeline.SetOnClick(unityEvent);

		startTime = 0;
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
	}

	void Update()
	{
		CheckExpiration();
		UpdatePayable();
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
		if (diff >= 0)
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

	void MakePayment()
	{
		if (wallet.GetAmount() >= OwedAmount())
		{
			TutorialManager.Instance.BroadcastSignal(TutorialManager.Actions.DEBT);
			wallet.Subtract(OwedAmount());
			receipt.Clear();
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
}