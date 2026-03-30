using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

namespace Sunspore
{
    public class ItemRecipe : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public ItemRecipeSO itemSO;

        [SerializeField] GameObject itemIconPrefab;
        [SerializeField] GameObject equalIconPrefab;
        [SerializeField] float iconSpacing;

        private bool canCraft;
        [SerializeField] UnityEngine.UI.Image backgroundImage;
        [SerializeField] Color normalColor = new Color(1f,1f,1f,1f);
        [SerializeField] Color selectedColor = new Color(0.8f,0.9f,1f,1f);

        public void OnPointerEnter(PointerEventData eventData)
        {
            canCraft = CraftingManager.Instance != null && CraftingManager.Instance.CanCraftRecipe(itemSO);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // no-op
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CraftingManager.Instance != null)
            {
                CraftingManager.Instance.SelectRecipe(itemSO, this);
            }
        }

        public void SetSelected(bool sel)
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<UnityEngine.UI.Image>();
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = sel ? selectedColor : normalColor;
            }
        }

        public void UpdateRecipeUI(ItemRecipeSO newItemSO)
        {
            itemSO = newItemSO;

            if (itemSO == null || itemSO.ingredients == null || itemSO.resultItem == null || itemIconPrefab == null)
            {
                Debug.LogError("ItemRecipe: Missing references in UpdateRecipeUI. Ensure itemSO, ingredients, resultItem and itemIconPrefab are assigned.");
                return;
            }

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            var hLayout = GetComponent<HorizontalLayoutGroup>();
            if (hLayout == null)
            {
                hLayout = gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            hLayout.spacing = iconSpacing;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;
            hLayout.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < itemSO.ingredients.Length; i++)
            {
                var ingredient = itemSO.ingredients[i];
                if (ingredient == null || ingredient.item == null)
                {
                    Debug.LogWarning($"ItemRecipe: missing ingredient or item at index {i} in recipe '{itemSO.recipeName}'.");
                    continue;
                }

                GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                if (newItemIcon.transform.childCount > 0)
                {
                    var img = newItemIcon.transform.GetChild(0).GetComponent<Image>();
                    if (img != null) img.sprite = ingredient.item.icon;
                }
                if (newItemIcon.transform.childCount > 1)
                {
                    var txt = newItemIcon.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null) txt.text = ingredient.amount.ToString();
                }
            }

            if (equalIconPrefab != null)
            {
                Instantiate(equalIconPrefab, transform);
            }

            for (int i = 0; i < itemSO.resultItem.Length; i++)
            {
                var result = itemSO.resultItem[i];
                if (result == null || result.item == null)
                {
                    Debug.LogWarning($"ItemRecipe: missing result or item at index {i} in recipe '{itemSO.recipeName}'.");
                    continue;
                }

                GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                if (newItemIcon.transform.childCount > 0)
                {
                    var img = newItemIcon.transform.GetChild(0).GetComponent<Image>();
                    if (img != null) img.sprite = result.item.icon;
                }
                if (newItemIcon.transform.childCount > 1)
                {
                    var txt = newItemIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (txt != null) txt.text = result.amount.ToString();
                }
            }
        }
    }
}
