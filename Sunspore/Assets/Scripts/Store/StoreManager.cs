using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Sunspore
{
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager Instance { get; private set; }
        public System.Action<bool, bool> OnSelectionChanged; // (hasSelection, processable)

        private StoreRecipeSO selectedRecipeSO;
        private StoreRecipe selectedRecipeUI;
        private StoreRecipeType selectedRecipeType;
        [SerializeField] StoreRecipeSO[] recipes;
        [SerializeField] GameObject recipeUIPrefab;
        [SerializeField] Transform recipeUIParent;

        [Header("Process Button UI")]
        [SerializeField] Button processButton;
        [SerializeField] Color buttonDefaultColor = Color.white;
        [SerializeField] Color buttonSelectedColor = Color.yellow;
        [SerializeField] Color buttonProcessableColor = Color.green;
        [SerializeField] Sprite buyButtonSprite;
        [SerializeField] Sprite sellButtonSprite;

        [SerializeField] TMP_Text wallet;
        [SerializeField] Wallet walletComponent;
        private decimal walletBalance;

        private List<ItemTypeAndAmount> items = new List<ItemTypeAndAmount>();

        [Header("Selected Recipe Preview")]
        [SerializeField] UnityEngine.UI.Image previewIcon;
        [SerializeField] TMP_Text previewName;
        [SerializeField] TMP_Text previewDescription;
        [SerializeField] GameObject previewContainer;

        private void Awake()
        {
            if (Instance == null) Instance = this; else Destroy(gameObject);

            // Auto-find Wallet component if inspector field is empty
            if (walletComponent == null)
            {
                walletComponent = FindObjectOfType<Wallet>();
            }

            // If a TMP text is assigned in the store UI and the Wallet exists, wire them so Wallet updates the text.
            if (walletComponent != null && wallet != null)
            {
                walletComponent.SetTextTarget(wallet);
            }

            // Clear preview contents so the panel shows empty until a selection is made
            if (previewIcon != null) previewIcon.gameObject.SetActive(false);
            if (previewName != null) previewName.text = "";
            if (previewDescription != null) previewDescription.text = "";
        }

        private void OnEnable()
        {
            selectedRecipeSO = null;
            selectedRecipeUI = null;
            selectedRecipeType = StoreRecipeType.buy;
            // Initialize wallet balance from text if possible
            if (wallet != null)
            {
                if (walletComponent == null)
                {
                    decimal.TryParse(wallet.text, out walletBalance);
                    UpdateWalletText();
                }
            }
            UpdateRecipeUI();
            OnSelectionChanged?.Invoke(false, false);
            SetButtonState(false, false);
            UpdateProcessButtonLabel();
            UpdateSelectedPreview(null);
        }

        public bool CanProcessRecipe(StoreRecipeSO recipeSO)
        {
            if (recipeSO == null) return false;

            // Buying recipes require enough credits (creditCost)
            if (recipeSO.recipeType == StoreRecipeType.buy)
            {
                decimal current = walletComponent != null ? walletComponent.GetAmount() : walletBalance;
                // If wallet is negative or zero, disallow purchases
                if (current <= 0m) return false;
                return current >= recipeSO.creditCost;
            }

            // Selling/processing recipes require inventory ingredients
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
                // Only create UI entries for the currently selected store tab (buy/sell)
                if (recipes[i].recipeType == selectedRecipeType)
                {
                    GameObject newRecipeUI = Instantiate(recipeUIPrefab, recipeUIParent);
                    newRecipeUI.name = recipes[i].recipeName;
                    // If the prefab has a StoreRecipe component, assign and initialize it immediately
                    var sr = newRecipeUI.GetComponent<StoreRecipe>();
                    if (sr != null)
                    {
                        sr.UpdateRecipeUI(recipes[i]);
                    }
                }
            }

            for (int i = 0; i < recipeUIParent.childCount; i++)
            {
                StoreRecipe storeRecipe = recipeUIParent.GetChild(i).GetComponent<StoreRecipe>();
                StoreRecipeSO storeRecipeSO = null;

                // Prefer the StoreRecipe's assigned SO (set during instantiation) to avoid mismatches when
                // multiple SOs share the same recipeName. Fallback to name lookup if not assigned.
                if (storeRecipe != null && storeRecipe.storeSO != null)
                {
                    storeRecipeSO = storeRecipe.storeSO;
                }
                else
                {
                    foreach (StoreRecipeSO recipe in recipes)
                    {
                        if (recipe.recipeName == recipeUIParent.GetChild(i).name)
                        {
                            storeRecipeSO = recipe;
                            break;
                        }
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

                // Ensure the StoreRecipe has the correct SO reference
                storeRecipe.storeSO = storeRecipeSO;

                // If UpdateRecipeUI wasn't called during instantiation, call it now
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

        public void SelectedRecipeType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                selectedRecipeType = StoreRecipeType.buy;
            }
            else
            {
                switch (type.ToLowerInvariant())
                {
                    case "buy":
                        selectedRecipeType = StoreRecipeType.buy;
                        break;
                    case "sell":
                        selectedRecipeType = StoreRecipeType.sell;
                        break;
                    default:
                        selectedRecipeType = StoreRecipeType.buy;
                        break;
                }
            }

            // clear selection when switching tabs
            selectedRecipeSO = null;
            if (selectedRecipeUI != null)
            {
                selectedRecipeUI.SetSelected(false);
                selectedRecipeUI = null;
            }
            OnSelectionChanged?.Invoke(false, false);
            SetButtonState(false, false);
            UpdateRecipeUI();
            UpdateProcessButtonLabel();
            UpdateSelectedPreview(null);
        }

        // Parameterless helpers for easy Button wiring in the Inspector
        public void SelectedRecipeType_Buy()
        {
            SelectedRecipeType("buy");
        }

        public void SelectedRecipeType_Sell()
        {
            SelectedRecipeType("sell");
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
            UpdateSelectedPreview(selectedRecipeSO);
        }

        public void ProcessSelectedRecipe()
        {
            if (selectedRecipeSO == null) return;
            if (!CanProcessRecipe(selectedRecipeSO))
            {
                Debug.Log("StoreManager: selected recipe cannot be processed right now.");
                SetButtonState(true, false);
                return;
            }

            if (selectedRecipeSO.recipeType == StoreRecipeType.buy)
            {
                // Deduct credits (creditCost) then give result items
                int price = selectedRecipeSO.creditCost;
                if (walletComponent != null)
                {
                    walletComponent.Subtract(price);
                }
                else
                {
                    walletBalance -= price;
                }

                if (selectedRecipeSO.resultItem != null && selectedRecipeSO.resultItem.Length > 0)
                {
                    InventoryManager.Instance.CraftItem(new List<ItemTypeAndAmount>(selectedRecipeSO.resultItem), new List<ItemTypeAndAmount>());
                }
            }
            else // sell/process
            {
                // remove ingredients via InventoryManager and give credits
                InventoryManager.Instance.CraftItem(new List<ItemTypeAndAmount>(), new List<ItemTypeAndAmount>(selectedRecipeSO.ingredients));
                AddCredits(selectedRecipeSO.creditReward);
            }

            // After processing, refresh UI and clear selection
            selectedRecipeUI = null;
            selectedRecipeSO = null;
            UpdateSelectedPreview(null);
            OnSelectionChanged?.Invoke(false, false);
            SetButtonState(false, false);
            UpdateRecipeUI();
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

        private void UpdateProcessButtonLabel()
        {
            if (processButton == null) return;
            // If sprites are provided, use them for the button image; otherwise update label text
            if (buyButtonSprite != null && sellButtonSprite != null)
            {
                processButton.image.sprite = selectedRecipeType == StoreRecipeType.buy ? buyButtonSprite : sellButtonSprite;
                return;
            }

            var tmp = processButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = selectedRecipeType == StoreRecipeType.buy ? "Buy" : "Sell";
            }
        }

        private void UpdateSelectedPreview(StoreRecipeSO recipeSO)
        {
            // Keep the preview container visible; only update contents.
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

        public bool IsRecipeSelected => selectedRecipeSO != null;

        public bool IsSelectedAndProcessable()
        {
            return selectedRecipeSO != null && CanProcessRecipe(selectedRecipeSO);
        }

        private void AddCredits(int amount)
        {
            if (walletComponent != null)
            {
                walletComponent.Add(amount);
            }
            else
            {
                walletBalance += amount;
            }
            UpdateWalletText();
        }

        private void UpdateWalletText()
        {
            if (wallet != null)
            {
                // If a Wallet component is assigned, it manages its own UI text.
                if (walletComponent == null)
                {
                    wallet.text = walletBalance.ToString();
                }
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

            if (selectedRecipeSO.recipeType == StoreRecipeType.buy)
            {
                ProcessSelectedRecipe();
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
