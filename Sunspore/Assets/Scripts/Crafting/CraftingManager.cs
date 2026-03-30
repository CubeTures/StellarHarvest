using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Sunspore
{
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance { get; private set; }
        public System.Action<bool, bool> OnSelectionChanged; // (hasSelection, craftable)
        private RecipeType selectedRecipeType;
        private ItemRecipeSO selectedRecipeSO;
        private ItemRecipe selectedRecipeUI;
        [SerializeField] ItemRecipeSO[] recipes;
        [SerializeField] GameObject recipeUIPrefab;
        [SerializeField] Transform recipeUIParent;

        [Header("Craft Button UI")]
        [SerializeField] Button craftButton;
        [SerializeField] Color craftButtonDefaultColor = Color.white;
        [SerializeField] Color craftButtonSelectedColor = Color.yellow;
        [SerializeField] Color craftButtonCraftableColor = Color.green;

        [Header("Selected Recipe Preview")]
        [SerializeField] UnityEngine.UI.Image previewIcon;
        [SerializeField] TMPro.TMP_Text previewName;
        [SerializeField] TMPro.TMP_Text previewDescription;
        [SerializeField] GameObject previewContainer;

        private List<ItemTypeAndAmount> items = new List<ItemTypeAndAmount>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            // Ensure the preview image and texts are cleared on startup so the panel appears empty.
            if (previewIcon != null) previewIcon.gameObject.SetActive(false);
            if (previewName != null) previewName.text = "";
            if (previewDescription != null) previewDescription.text = "";
        }

        private void OnEnable()
        {
            selectedRecipeType = RecipeType.all;
            selectedRecipeSO = null;
            selectedRecipeUI = null;
            UpdateRecipeUI();
            OnSelectionChanged?.Invoke(false, false);
            SetCraftButtonState(false, false);
        }

        public bool CanCraftRecipe(ItemRecipeSO recipeSO)
        {
            items = InventoryManager.Instance.GetAllItems();

            int matchingIngredients = 0;

            foreach (ItemTypeAndAmount ingredient in recipeSO.ingredients)
            {
                foreach (ItemTypeAndAmount foundItemAndCount in items)
                {
                    if (foundItemAndCount.item == ingredient.item && foundItemAndCount.amount >= ingredient.amount)
                    {
                        matchingIngredients++;
                        break;
                    }
                }
            }
            return matchingIngredients == recipeSO.ingredients.Length;
        }

        private void UpdateRecipeUI()
        {
            foreach (Transform child in recipeUIParent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < recipes.Length; i++)
            {
                if (recipes[i].recipeType == selectedRecipeType || selectedRecipeType == RecipeType.all)
                {
                    GameObject newRecipeUI = Instantiate(recipeUIPrefab, recipeUIParent);
                    // use the scriptable object's recipeName so later lookup matches
                    newRecipeUI.name = recipes[i].recipeName;
                }
            }
            for (int i = 0; i < recipeUIParent.childCount; i++)
            {
                ItemRecipe itemRecipe = recipeUIParent.GetChild(i).GetComponent<ItemRecipe>();
                ItemRecipeSO itemRecipeSO = null;

                foreach (ItemRecipeSO recipe in recipes)
                {
                    if (recipe.recipeName == recipeUIParent.GetChild(i).name)
                    {
                        itemRecipeSO = recipe;
                        break;
                    }
                }
                if (itemRecipe == null)
                {
                    Debug.LogWarning($"CraftingManager: missing ItemRecipe component on '{recipeUIParent.GetChild(i).name}'");
                    continue;
                }

                if (itemRecipeSO == null)
                {
                    Debug.LogWarning($"CraftingManager: no ItemRecipeSO found matching UI name '{recipeUIParent.GetChild(i).name}'");
                    continue;
                }
                itemRecipe.UpdateRecipeUI(itemRecipeSO);

                // Ensure visual selection state is cleared unless it matches the selected recipe
                if (selectedRecipeSO != null && itemRecipeSO == selectedRecipeSO)
                {
                    selectedRecipeUI = itemRecipe;
                    itemRecipe.SetSelected(true);
                }
                else
                {
                    itemRecipe.SetSelected(false);
                }

            }
        }

        public void SelectRecipe(ItemRecipeSO recipeSO, ItemRecipe recipeUI)
        {
            // unselect previous
            if (selectedRecipeUI != null && selectedRecipeUI != recipeUI)
            {
                selectedRecipeUI.SetSelected(false);
            }

            selectedRecipeSO = recipeSO;
            selectedRecipeUI = recipeUI;
            if (selectedRecipeUI != null) selectedRecipeUI.SetSelected(true);
            bool hasSelection = selectedRecipeSO != null;
            bool craftable = hasSelection && CanCraftRecipe(selectedRecipeSO);
            SetCraftButtonState(hasSelection, craftable);
            OnSelectionChanged?.Invoke(hasSelection, craftable);

            UpdateSelectedPreview(selectedRecipeSO);
        }

        private void UpdateSelectedPreview(ItemRecipeSO recipeSO)
        {
            // Only update the preview contents; do not toggle the container visibility.
            if (recipeSO == null)
            {
                if (previewIcon != null)
                {
                    previewIcon.sprite = null;
                    previewIcon.gameObject.SetActive(false);
                }
                if (previewName != null) previewName.text = "";
                if (previewDescription != null) previewDescription.text = "";
                return;
            }

            ItemSO itemToShow = null;
            if (recipeSO.resultItem != null && recipeSO.resultItem.Length > 0)
            {
                itemToShow = recipeSO.resultItem[0].item;
            }
            else if (recipeSO.ingredients != null && recipeSO.ingredients.Length > 0)
            {
                itemToShow = recipeSO.ingredients[0].item;
            }

            if (itemToShow != null)
            {
                if (previewIcon != null)
                {
                    previewIcon.sprite = itemToShow.icon;
                    previewIcon.gameObject.SetActive(true);
                }
                if (previewName != null) previewName.text = recipeSO.recipeName ?? itemToShow.displayName;
                if (previewDescription != null) previewDescription.text = !string.IsNullOrEmpty(recipeSO.description) ? recipeSO.description : (itemToShow.description ?? "");
            }
            else
            {
                if (previewIcon != null)
                {
                    previewIcon.sprite = null;
                    previewIcon.gameObject.SetActive(false);
                }
                if (previewName != null) previewName.text = "";
                if (previewDescription != null) previewDescription.text = "";
            }
        }

        public void CraftSelectedRecipe()
        {
            if (selectedRecipeSO == null) return;

            if (CanCraftRecipe(selectedRecipeSO))
            {
                InventoryManager.Instance.CraftItem(new List<ItemTypeAndAmount>(selectedRecipeSO.resultItem), new List<ItemTypeAndAmount>(selectedRecipeSO.ingredients));
                // After crafting, refresh UI and clear selection if recipe no longer craftable
                selectedRecipeUI = null;
                selectedRecipeSO = null;
                UpdateSelectedPreview(null);
                OnSelectionChanged?.Invoke(false, false);
                SetCraftButtonState(false, false);
                UpdateRecipeUI();
            }
            else
            {
                Debug.Log("CraftingManager: selected recipe cannot be crafted right now.");
                // keep selection but update button to show it's not craftable
                SetCraftButtonState(true, false);
            }
        }

        private void SetCraftButtonState(bool hasSelection, bool craftable)
        {
            if (craftButton == null) return;
            var img = craftButton.image;
            if (!hasSelection)
            {
                img.color = craftButtonDefaultColor;
            }
            else if (craftable)
            {
                img.color = craftButtonCraftableColor;
            }
            else
            {
                img.color = craftButtonSelectedColor;
            }
        }

        public bool IsRecipeSelected => selectedRecipeSO != null;

        public bool IsSelectedAndCraftable()
        {
            return selectedRecipeSO != null && CanCraftRecipe(selectedRecipeSO);
        }

        public void SelectedRecipeType(string type)
        {
            switch (type)
            {
                case "all":
                    selectedRecipeType = RecipeType.all;
                    break;
                case "tools":
                    selectedRecipeType = RecipeType.tools;
                    break;
                case "weapons":
                    selectedRecipeType = RecipeType.weapons;
                    break;
            }
            UpdateRecipeUI();
        }

        // Unity-friendly wrapper so Buttons can call this method from the Inspector
        // Pass the ItemRecipeSO asset as the OnClick argument.
        public void SelectedRecipe(ItemRecipeSO recipeSO)
        {
            if (recipeSO == null)
            {
                SelectRecipe(null, null);
                return;
            }

            // find the matching ItemRecipe UI under recipeUIParent
            ItemRecipe foundUI = null;
            for (int i = 0; i < recipeUIParent.childCount; i++)
            {
                var child = recipeUIParent.GetChild(i);
                if (child.name == recipeSO.recipeName)
                {
                    foundUI = child.GetComponent<ItemRecipe>();
                    break;
                }
            }

            SelectRecipe(recipeSO, foundUI);
        }
    }
}
