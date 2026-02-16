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

            if (itemSO == null || itemSO.ingredients == null || itemSO.resultItem == null || itemIconPrefab == null || equalIconPrefab == null)
            {
                Debug.LogError("ItemRecipe: Missing references in UpdateRecipeUI.");
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
                GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                newItemIcon.transform.GetChild(0).GetComponent<Image>().sprite = itemSO.ingredients[i].item.icon;
                newItemIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = itemSO.ingredients[i].amount.ToString();
            }
            Instantiate(equalIconPrefab, transform);
            for (int i = 0; i < itemSO.resultItem.Length; i++)
            {
                GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                newItemIcon.transform.GetChild(0).GetComponent<Image>().sprite = itemSO.resultItem[i].item.icon;
                newItemIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = itemSO.resultItem[i].amount.ToString();
            }
        }
    }
}
