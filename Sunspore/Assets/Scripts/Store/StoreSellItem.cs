using TMPro;
using UnityEngine;
using UnityEngine.UI;

class StoreSellItem: MonoBehaviour
{
	[SerializeField] Image icon;
	[SerializeField] TMP_Text titleText, costText, quantityText;

	StoreSell storeSell;
	public StoreItem storeItem
	{
		get; private set;	
	}
	public int quantity
	{
		get; private set;
	}

	public void SetStoreItem(StoreSell storeSell, StoreItem storeItem)
	{
		this.storeSell = storeSell;
		this.storeItem = storeItem;
		quantity = storeItem.amount;
	}

	public void Increment()
	{
		if (quantity < storeItem.amount)
		{
			quantity += 1;
		}
	}

	public void Decrement()
	{
		if (quantity > 0)
		{
			quantity -= 1;
			if (quantity <= 0)
			{
				RemoveSelf();
			}
		}
	}

	void RemoveSelf()
	{
		storeSell.RemoveStoreSellItem(this);
	}

	public int GetReward()
	{
		return storeItem.money * quantity;
	}

	void Update()
	{
		icon.sprite = storeItem.item.icon;
		titleText.text = storeItem.item.displayName;
		costText.text = $"{storeItem.money} credits";
		quantityText.text = quantity.ToString();
	}
}