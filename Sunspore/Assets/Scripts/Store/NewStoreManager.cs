using System.Collections.Generic;
using Sunspore;
using UnityEngine;
using System.Linq;
using NUnit.Framework.Interfaces;
using System;

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
	[SerializeField] ToggleButtonGroup filters;

	[Serializable]
	public enum StoreMode
	{
		BUY,
		SELL,
	}

	StoreMode mode = StoreMode.SELL;

	[SerializeField] StoreRecipeSO[] recipes;
	List<InventorySlot> items;
	bool visible = false;
	public bool IsOpen => visible;
	ItemCategory currentFilter = ItemCategory.All;


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
		filters.Reset();
		SFXManager.Play("inventoryopenclose");
		items = CollectItems();
		PopulateSidebars();
		visible = true;
		visuals.SetActive(true);
	}

	public void Close()
	{
		SFXManager.Play("inventoryopenclose");
		visible = false;
		visuals.SetActive(false);
		currentFilter = ItemCategory.All;
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
		if (mode == StoreMode.BUY)
		{
			storeSell.Clear();
			storeSell.gameObject.SetActive(false);
			storeBuy.Populate(
				recipes
					.Where(r => FilterSidebarContents(StoreRecipeType.buy, r))
					.ToList(),
				items
			);
			storeBuy.Filter(currentFilter);
			storeBuy.gameObject.SetActive(true);
		}
		else
		{
			storeBuy.Clear();
			storeBuy.gameObject.SetActive(false);
			storeSell.Populate(
				recipes
					.Where(r => FilterSidebarContents(StoreRecipeType.sell, r))
					.ToList(),
				items
			);
			storeSell.Filter(currentFilter);
			storeSell.gameObject.SetActive(true);
		}
	}

	bool FilterSidebarContents(StoreRecipeType sidebar, StoreRecipeSO recipe)
	{
		bool isTutorial = TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive;
		bool availableDuringTutorial = isTutorial ? ((recipe.name.Contains("Glowbud") && recipe.creditCost > 0) || (recipe.name.Contains("Bloomburst") && recipe.creditReward > 0 && recipe.category == StoreRecipeCategory.Plants)) : true;
		return availableDuringTutorial && recipe.recipeType == sidebar && !recipe.disabled;
	}

	public void SetStoreMode(StoreMode mode)
	{
		this.mode = mode;
		PopulateSidebars();
	}

	public void SetStoreModeBuy()
	{
		SetStoreMode(StoreMode.BUY);
	}

	public void SetStoreModeSell()
	{
		SetStoreMode(StoreMode.SELL);
	}

	public void SetFilter(ItemCategory cat)
	{
		currentFilter = cat;
		storeBuy.Filter(cat);
		storeSell.Filter(cat);
	}

	public void SetFilterAll()
	{
		SetFilter(ItemCategory.All);
	}

	public void SetFilterTools()
	{
		SetFilter(ItemCategory.Tools);
	}

	public void SetFilterPlants()
	{
		SetFilter(ItemCategory.Plants);
	}

	public void SetFilterWeapons()
	{
		SetFilter(ItemCategory.Weapons);
	}
}