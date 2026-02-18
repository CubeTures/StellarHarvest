using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Sunspore
{
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager Instance { get; private set; }
        public System.Action<bool, bool> OnSelectionChanged; // (hasSelection, processable)

        private StoreRecipeSO selectedRecipeSO;
        private StoreRecipe selectedRecipeUI;
        [SerializeField] StoreRecipeSO[] recipes;
        [SerializeField] GameObject recipeUIPrefab;
        [SerializeField] Transform recipeUIParent;

        [Header("Process Button UI")]
        [SerializeField] Button processButton;
        [SerializeField] Color buttonDefaultColor = Color.white;
        [SerializeField] Color buttonSelectedColor = Color.yellow;
        [SerializeField] Color buttonProcessableColor = Color.green;

        [SerializeField] Text wallet;
        private decimal walletBalance;

        private List<ItemTypeAndAmount> items = new List<ItemTypeAndAmount>();

        private void Awake()
        {
            if (Instance == null) Instance = this; else Destroy(gameObject);
        }

        private void OnEnable()
        {
            selectedRecipeSO = null;
            selectedRecipeUI = null;
            // Initialize wallet balance from text if possible
            if (wallet != null)
            {
                decimal.TryParse(wallet.text, out walletBalance);
                walletBalance = -50000;
                UpdateWalletText();
            }
            UpdateRecipeUI();
            OnSelectionChanged?.Invoke(false, false);
            SetButtonState(false, false);
        }

        public bool CanProcessRecipe(StoreRecipeSO recipeSO)
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
            if (recipeUIParent == null) return;

            foreach (Transform child in recipeUIParent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < recipes.Length; i++)
            {
                GameObject newRecipeUI = Instantiate(recipeUIPrefab, recipeUIParent);
                newRecipeUI.name = recipes[i].recipeName;
            }

            for (int i = 0; i < recipeUIParent.childCount; i++)
            {
                StoreRecipe storeRecipe = recipeUIParent.GetChild(i).GetComponent<StoreRecipe>();
                StoreRecipeSO storeRecipeSO = null;

                foreach (StoreRecipeSO recipe in recipes)
                {
                    if (recipe.recipeName == recipeUIParent.GetChild(i).name)
                    {
                        storeRecipeSO = recipe;
                        break;
                    }
                }
                if (storeRecipe == null)
                {
                    Debug.LogWarning($"StoreManager: missing StoreRecipe component on '{recipeUIParent.GetChild(i).name}'");
                    continue;
                }

                if (storeRecipeSO == null)
                {
                    Debug.LogWarning($"StoreManager: no StoreRecipeSO found matching UI name '{recipeUIParent.GetChild(i).name}'");
                    continue;
                }
                storeRecipe.UpdateRecipeUI(storeRecipeSO);

                if (selectedRecipeSO != null && storeRecipeSO == selectedRecipeSO)
                {
                    selectedRecipeUI = storeRecipe;
                    storeRecipe.SetSelected(true);
                }
                else
                {
                    storeRecipe.SetSelected(false);
                }
            }
        }

        public void SelectRecipe(StoreRecipeSO recipeSO, StoreRecipe recipeUI)
        {
            if (selectedRecipeUI != null && selectedRecipeUI != recipeUI)
            {
                selectedRecipeUI.SetSelected(false);
            }

            selectedRecipeSO = recipeSO;
            selectedRecipeUI = recipeUI;
            if (selectedRecipeUI != null) selectedRecipeUI.SetSelected(true);
            bool hasSelection = selectedRecipeSO != null;
            bool processable = hasSelection && CanProcessRecipe(selectedRecipeSO);
            SetButtonState(hasSelection, processable);
            OnSelectionChanged?.Invoke(hasSelection, processable);
        }

        public void ProcessSelectedRecipe()
        {
            if (selectedRecipeSO == null) return;

            if (CanProcessRecipe(selectedRecipeSO))
            {
                // remove ingredients via InventoryManager (reuse CraftItem with empty result list)
                InventoryManager.Instance.CraftItem(new List<ItemTypeAndAmount>(), new List<ItemTypeAndAmount>(selectedRecipeSO.ingredients));

                if (wallet != null)
                {
                    AddCredits(selectedRecipeSO.creditReward);
                }

                // After processing, refresh UI and clear selection if recipe no longer processable
                selectedRecipeUI = null;
                selectedRecipeSO = null;
                OnSelectionChanged?.Invoke(false, false);
                SetButtonState(false, false);
                UpdateRecipeUI();
            }
            else
            {
                Debug.Log("StoreManager: selected recipe cannot be processed right now.");
                SetButtonState(true, false);
            }
        }

        private void SetButtonState(bool hasSelection, bool processable)
        {
            if (processButton == null) return;
            var img = processButton.image;
            if (!hasSelection)
            {
                img.color = buttonDefaultColor;
            }
            else if (processable)
            {
                img.color = buttonProcessableColor;
            }
            else
            {
                img.color = buttonSelectedColor;
            }
        }

        public bool IsRecipeSelected => selectedRecipeSO != null;

        public bool IsSelectedAndProcessable()
        {
            return selectedRecipeSO != null && CanProcessRecipe(selectedRecipeSO);
        }

        private void AddCredits(int amount)
        {
            walletBalance += amount;
            UpdateWalletText();
        }

        private void UpdateWalletText()
        {
            if (wallet != null)
            {
                wallet.text = walletBalance.ToString();
            }
        }
        public void SelectedRecipe(StoreRecipeSO recipeSO)
        {
            if (recipeSO == null)
            {
                SelectRecipe(null, null);
                return;
            }

            StoreRecipe foundUI = null;
            for (int i = 0; i < recipeUIParent.childCount; i++)
            {
                var child = recipeUIParent.GetChild(i);
                if (child.name == recipeSO.recipeName)
                {
                    foundUI = child.GetComponent<StoreRecipe>();
                    break;
                }
            }

            SelectRecipe(recipeSO, foundUI);
        }

        // Sell the currently selected inventory item (uses InventoryManager selection)
        public void SellSelectedItem()
        {
            if (selectedRecipeSO == null)
            {
                Debug.Log("StoreManager: No recipe selected — will not sell. Select a recipe first.");
                return;
            }

            var all = InventoryManager.Instance.GetAllItems();
            if (all == null || all.Count == 0)
            {
                Debug.Log("StoreManager: No items in inventory to sell.");
                return;
            }

            // Find the first inventory item that matches one of the selected recipe's ingredients
            ItemSO match = null;
            foreach (ItemTypeAndAmount ingredient in selectedRecipeSO.ingredients)
            {
                foreach (var it in all)
                {
                    if (it.item == ingredient.item && it.amount > 0)
                    {
                        match = it.item;
                        break;
                    }
                }
                if (match != null) break;
            }

            if (match == null)
            {
                Debug.Log($"StoreManager: No inventory items match the selected recipe '{selectedRecipeSO.recipeName}'.");
                return;
            }

            int reward = selectedRecipeSO != null ? selectedRecipeSO.creditReward : match.sellPrice;
            if (wallet != null)
            {
                AddCredits(reward);
            }

            InventoryManager.Instance.SellItems(new List<ItemTypeAndAmount> { new ItemTypeAndAmount(match, 1) });
            UpdateRecipeUI();
        }
    }
}
