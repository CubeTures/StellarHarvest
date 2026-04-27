using System.Collections.Generic;
using Sunspore;
using UnityEngine;
using System.Linq;
using NUnit.Framework.Interfaces;

class NewStoreManager : MonoBehaviour
{
	public static NewStoreManager Instance
	{
		get; private set;
	}

	[SerializeField] InventoryManager im;
	[SerializeField] GameObject visuals;
	[SerializeField] StoreBuy storeBuy;
	[SerializeField] StoreSell storeSell;
	[SerializeField] GameObject storeItemPrefab;

	public enum StoreMode
	{
		BUY,
		SELL,
	}

	StoreMode mode = StoreMode.SELL;

	[SerializeField] StoreRecipeSO[] recipes;
	List<InventorySlot> items;
	bool visible = false;
	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		storeBuy.SetStoreItemPrefab(storeItemPrefab);
		storeSell.SetStoreItemPrefab(storeItemPrefab);
		visuals.SetActive(visible);
	}

	public bool Toggle()
	{
		SFXManager.Play("inventoryopenclose");
		if (visible)
		{
			Close();
			return false;
		}
		else
		{
			Open();
			return true;
		}
	}

	void Open()
	{
		items = CollectItems();
		PopulateSidebars();
		visible = true;
		visuals.SetActive(true);
	}

	void Close()
	{
		visible = false;
		visuals.SetActive(false);
	}

	public List<InventorySlot> UpdateItems()
	{
		items = CollectItems();
		return items;
	}

	List<InventorySlot> CollectItems()
	{
		List<InventorySlot> items = new();
		items.AddRange(ExtractSlots(im.slots));
		items.AddRange(ExtractSlots(im.hotbarSlots));
		return items;
	}

	List<InventorySlot> ExtractSlots(GameObject[] objs)
	{
		return objs
			.Select(o => o.GetComponent<InventorySlot>())
			.Where(s => s.heldItem != null)
			.ToList();
	}

	void PopulateSidebars()
	{
		storeBuy.Populate(
			recipes
				.Where(r => r.recipeType == StoreRecipeType.buy && !r.disabled)
				.ToList(),
			items
		);
		storeSell.Populate(
			recipes
				.Where(r => r.recipeType == StoreRecipeType.sell && !r.disabled)
				.ToList(),
			items
		);
	}

	public void SetStoreMode(StoreMode mode)
	{
		this.mode = mode;
	}
}