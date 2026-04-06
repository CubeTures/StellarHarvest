using System.Collections.Generic;
using Sunspore;
using UnityEngine;

public class CraftStep : TutorialStep
{
    [System.Serializable]
    private struct CraftRequirement
    {
        public ItemSO item;
        public int amount;
    }

    [SerializeField]
    private InventoryManager inventoryManager;

    [SerializeField]
    private List<CraftRequirement> craftRequirements = new();

    private readonly Dictionary<ItemSO, int> _requirementsByItem = new();
    private readonly Dictionary<ItemSO, int> _initialAmountsByItem = new();

    public override void Begin()
    {
        BuildRequirementsDictionary();
        _initialAmountsByItem.Clear();

        foreach (var requirement in _requirementsByItem)
        {
            _initialAmountsByItem[requirement.Key] = GetCurrentAmount(requirement.Key);
        }

        Debug.Log("Dialog: Craft a sword using some of your harvested plants.");
    }

    public override void Check()
    {
        foreach (var requirement in _requirementsByItem)
        {
            ItemSO item = requirement.Key;
            int requiredIncrease = requirement.Value;

            int initialAmount = 0;
            _initialAmountsByItem.TryGetValue(item, out initialAmount);

            int currentAmount = GetCurrentAmount(item);
            if (currentAmount >= initialAmount + requiredIncrease)
            {
                TutorialManager.Instance.CompleteStep();
                return;
            }
        }
    }

    public override void End()
    {
        // award player money or plants for completing the tutorial if desired
    }

    private void BuildRequirementsDictionary()
    {
        _requirementsByItem.Clear();

        foreach (var requirement in craftRequirements)
        {
            if (requirement.item == null)
            {
                continue;
            }

            if (_requirementsByItem.ContainsKey(requirement.item))
            {
                _requirementsByItem[requirement.item] += requirement.amount;
            }
            else
            {
                _requirementsByItem.Add(requirement.item, requirement.amount);
            }
        }
    }

    private int GetCurrentAmount(ItemSO craftTarget)
    {
        if (inventoryManager == null || craftTarget == null)
        {
            return 0;
        }

        List<ItemTypeAndAmount> allItems = inventoryManager.GetAllItems();
        foreach (var entry in allItems)
        {
            if (entry.item == craftTarget)
            {
                return entry.amount;
            }
        }

        return 0;
    }
}
