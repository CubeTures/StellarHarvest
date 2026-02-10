using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace Sunspore
{
    public class ItemRecipe : MonoBehaviour
    {
        public ItemRecipeSO itemSO;

        [SerializeField] GameObject itemIconPrefab;
        [SerializeField] GameObject plusIconPrefab;
        [SerializeField] GameObject equalIconPrefab;

        private bool canCraft;

        public void OnPointerEnter()
        {
            canCraft = CraftingManager.Instance.CanCraftRecipe(itemSO);
        }

        public void OnPointerExit()
        {
            
        }

        public void OnPointerClick()
        {
            if (CraftingManager.Instance.CanCraftRecipe(itemSO))
            {
                InventoryManager.Instance.CraftItem(new List<ItemTypeAndAmount>(itemSO.resultItem), new List<ItemTypeAndAmount>(itemSO.ingredients));
                canCraft = CraftingManager.Instance.CanCraftRecipe(itemSO);
                if (!canCraft)
                {
                }
            }
        }

        public void UpdateRecipeUI(ItemRecipeSO newItemSO)
        {
            itemSO = newItemSO;

            if (itemSO == null || itemSO.ingredients == null || itemSO.resultItem == null || itemIconPrefab == null || plusIconPrefab == null || equalIconPrefab == null)
            {
                Debug.LogError("ItemRecipe: Missing references in UpdateRecipeUI.");
                return;
            }

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < itemSO.ingredients.Length; i++)
            {
                GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                newItemIcon.transform.GetChild(0).GetComponent<Image>().sprite = itemSO.ingredients[i].item.icon;
                newItemIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = itemSO.ingredients[i].amount.ToString();

                if (i < itemSO.ingredients.Length - 1)
                {
                    Instantiate(plusIconPrefab, transform);
                }
            }
            Instantiate(equalIconPrefab, transform);
            for (int i = 0; i < itemSO.resultItem.Length; i++)
            {
                GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                newItemIcon.transform.GetChild(0).GetComponent<Image>().sprite = itemSO.resultItem[i].item.icon;
                newItemIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = itemSO.resultItem[i].amount.ToString();

                if (i < itemSO.resultItem.Length - 1)
                {
                    Instantiate(plusIconPrefab, transform);
                }
            }
        }
    }
}
