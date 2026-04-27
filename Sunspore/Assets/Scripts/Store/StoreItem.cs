using System.Collections.Generic;
using Sunspore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

class StoreItem : MonoBehaviour
{
	// logic for clicking and dragging in store
	public delegate void OnClick(StoreItem storeItem);
	Button button;

	[SerializeField] Image image;
	[SerializeField] Image amountBg, checkmark;
	[SerializeField] TMP_Text amountText, moneyText;

	NewStoreManager.StoreMode mode; // switches item count to owned check
	StoreRecipeSO recipe;
	public ItemSO item
	{
		get; private set;
	}
	public int money
	{
		get; private set;
	}
	public int amount
	{
		get; private set;
	}

	public void SetFields(NewStoreManager.StoreMode mode, StoreRecipeSO recipe, List<InventorySlot> items)
	{
		button = GetComponent<Button>();
		this.mode = mode;
		this.recipe = recipe;
		item = ExtractItem(recipe);
		money = ExtractCost(recipe);
		amount = FindAmountHeld(items);
	}

	ItemSO ExtractItem(StoreRecipeSO recipe)
	{
		return recipe.GetItem();
	}

	int ExtractCost(StoreRecipeSO recipe)
	{
		return mode == NewStoreManager.StoreMode.BUY ? recipe.creditCost : recipe.creditReward;
	}

	int FindAmountHeld(List<InventorySlot> items)
	{
		foreach (InventorySlot slot in items)
		{
			if (slot.heldItem != null)
			{
				InventoryItem heldItem = slot.heldItem.GetComponent<InventoryItem>();
				if (heldItem != null)
				{
					ItemSO itemSO = heldItem.itemScriptableObject;
					if (itemSO != null && itemSO.displayName == item.displayName)
					{
						return slot.quantity;
					}	
				} 
			}
		}

		return 0;
	}

	public void UpdateAmountHeld(List<InventorySlot> items)
	{
		amount = FindAmountHeld(items);
	}

	public void RegisterOnClick(OnClick onClick)
	{
		UnityAction action = new(() => onClick(this));
		button.onClick.AddListener(action);
	}

	void Update()
	{
		image.sprite = item.icon;
		amountText.text = amount.ToString();
		moneyText.text = money.ToString();

		bool buyMode = mode == NewStoreManager.StoreMode.BUY;
		bool any = amount != 0;
		amountBg.enabled = any;
		checkmark.enabled = buyMode && any;
		amountText.enabled = !buyMode && any;
	}
}