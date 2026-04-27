using System.Collections.Generic;
using Sunspore;
using UnityEngine;

class StoreSell: MonoBehaviour
{
	// logic and display for sell sidebar
	List<StoreRecipeSO> recipes;
	List<StoreItem> storeItems;
	List<StoreSellItem> storeSellItems;
	GameObject storeItemPrefab;

	public void SetStoreItemPrefab(GameObject storeItemPrefab)
	{
		this.storeItemPrefab = storeItemPrefab;
	}

	public void Populate(List<StoreRecipeSO> recipes, List<InventorySlot> items)
	{
		Clear();
	}

	void Clear()
	{
		// TODO
	}

	List<StoreItem> CreateStoreItems()
	{
		List<StoreItem> storeItems = new();
		// TODO instantiate to store items from item so
		return storeItems;
	}
}