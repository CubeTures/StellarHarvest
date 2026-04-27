using System.Collections.Generic;
using System.Linq;
using Sunspore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class StoreBuy : MonoBehaviour
{
	[SerializeField] RectTransform content;

	List<StoreItem> storeItems = new();
	GameObject storeItemPrefab;

	StoreItem selected;

	[SerializeField] Image previewImage;
	[SerializeField] GameObject previewOwned;
	[SerializeField] TMP_Text previewTitle, previewDesc, previewPay;

	public void SetStoreItemPrefab(GameObject storeItemPrefab)
	{
		this.storeItemPrefab = storeItemPrefab;
	}

	public void Populate(List<StoreRecipeSO> recipes, List<InventorySlot> items)
	{
		Clear();
		storeItems = CreateStoreItems(recipes, items);
		selected = storeItems.First();
	}

	public void Clear()
	{
		foreach(var i in storeItems)
		{
			Destroy(i.gameObject);
		}
		storeItems.Clear();
	}

	List<StoreItem> CreateStoreItems(List<StoreRecipeSO> recipes, List<InventorySlot> items)
	{
		List<StoreItem> sis = new();

		foreach (var recipe in recipes)
		{
			GameObject storeItemObj = Instantiate(storeItemPrefab, content);
			StoreItem storeItem = storeItemObj.GetComponent<StoreItem>();
			storeItem.SetFields(NewStoreManager.StoreMode.BUY, recipe, items);
			storeItem.RegisterOnClick(Select);
			sis.Add(storeItem);
		}

		sis.Sort((a, b) => a.amount - b.amount);
		return sis;
	}

	public void Select(StoreItem storeItem)
	{
		selected = storeItem;
	}

	public void BuySelected()
	{
		if (Wallet.Instance.GetAmount() < selected.money) return;

		Wallet.Instance.Subtract(selected.money);
		InventoryManager.Instance.AddItem(selected.item, 1);

		// update owned
		var items = NewStoreManager.Instance.UpdateItems();
		foreach (StoreItem storeItem in storeItems)
		{
			storeItem.UpdateAmountHeld(items);
		}
	}

	void Update()
	{
		previewImage.sprite = selected.item.icon;
		previewOwned.SetActive(selected.amount > 0);
		previewTitle.text = selected.item.displayName;
		previewDesc.text = selected.item.description;
		previewPay.text = $"Buy for {selected.money}";
	}
}