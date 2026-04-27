using System.Collections.Generic;
using System.Linq;
using Sunspore;
using TMPro;
using UnityEngine;

class StoreSell : MonoBehaviour
{
	[SerializeField] RectTransform content;
	[SerializeField] RectTransform contentSell;
	[SerializeField] GameObject storeSellItemPrefab;
	[SerializeField] TMP_Text previewSell;

	List<StoreRecipeSO> recipes;
	List<StoreItem> storeItems = new();
	List<StoreSellItem> storeSellItems = new();
	GameObject storeItemPrefab;

	public void SetStoreItemPrefab(GameObject storeItemPrefab)
	{
		this.storeItemPrefab = storeItemPrefab;
	}

	public void Populate(List<StoreRecipeSO> recipes, List<InventorySlot> items)
	{
		this.recipes = recipes;
		Clear();
		storeItems = CreateStoreItems(recipes, items);
		Update();
	}

	public void Clear()
	{
		foreach (var si in storeItems)
		{
			Destroy(si.gameObject);
		}
		storeItems.Clear();

		foreach (var ssi in storeSellItems)
		{
			Destroy(ssi.gameObject);
		}
		storeSellItems.Clear();
	}

	List<StoreItem> CreateStoreItems(List<StoreRecipeSO> recipes, List<InventorySlot> items)
	{
		List<StoreItem> sis = new();

		foreach (var slot in items)
		{
			ItemSO item = slot.GetItemSO();
			if (item == null) continue;

			StoreRecipeSO recipe = recipes.Find(r => r.GetItem().displayName == item.displayName);
			if (recipe == null) continue;

			GameObject storeItemObj = Instantiate(storeItemPrefab, content);
			StoreItem storeItem = storeItemObj.GetComponent<StoreItem>();
			storeItem.SetFields(NewStoreManager.StoreMode.SELL, recipe, items);
			storeItem.RegisterOnClick(Select);
			sis.Add(storeItem);
		}

		sis.Sort((a, b) => a.amount - b.amount);
		return sis;
	}

	void Select(StoreItem si)
	{
		if (storeSellItems.Any(ssi => ssi.storeItem == si)) return;

		var ssi = CreateStoreSellItem(si);
		storeSellItems.Add(ssi);
	}

	StoreSellItem CreateStoreSellItem(StoreItem si)
	{
		GameObject storeSellItemObj = Instantiate(storeSellItemPrefab, contentSell);
		StoreSellItem ssi = storeSellItemObj.GetComponent<StoreSellItem>();
		ssi.SetStoreItem(this, si);
		return ssi;
	}

	public void RemoveStoreSellItem(StoreSellItem ssi)
	{
		storeSellItems.Remove(ssi);
		Destroy(ssi.gameObject);
	}

	void Update()
	{
		previewSell.text = $"Sell for {storeSellItems.Aggregate(0, (acc, ssi) => acc + ssi.GetReward())}";
	}

	public void SellSelected()
	{
		List<ItemTypeAndAmount> sale = new();
		foreach (var ssi in storeSellItems)
		{
			sale.Add(new(ssi.storeItem.item, ssi.quantity));
		}

		InventoryManager.Instance.SellItems(sale);

		int reward = storeSellItems.Aggregate(0, (acc, ssi) => acc + ssi.GetReward());
		Wallet.Instance.Add(reward);

		foreach (var ssi in storeSellItems)
		{
			Destroy(ssi.gameObject);
		}
		storeSellItems.Clear();

		var items = NewStoreManager.Instance.UpdateItems();
		List<StoreItem> toRemove = new();
		foreach(var si in storeItems)
		{
			si.UpdateAmountHeld(items);
			if (si.amount == 0)
			{
				toRemove.Add(si);
			}
		}
		foreach(var remove in toRemove)
		{
			storeItems.Remove(remove);
			Destroy(remove.gameObject);
		}
	}
}