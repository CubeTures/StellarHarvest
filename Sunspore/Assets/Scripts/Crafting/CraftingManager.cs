using UnityEngine;
using System.Collections.Generic;

namespace Sunspore
{
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance { get; private set; }
        private RecipeType selectedRecipeType;
        [SerializeField] ItemRecipeSO[] recipes;
        [SerializeField] GameObject recipeUIPrefab;
        [SerializeField] Transform recipeUIParent;

        private List<ItemTypeAndAmount> items = new List<ItemTypeAndAmount>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            selectedRecipeType = RecipeType.tools;
            UpdateRecipeUI();
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
                if (recipes[i].recipeType == selectedRecipeType)
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

            }
        }

        public void SelectedRecipeType(string type)
        {
            switch (type)
            {
                case "tools":
                    selectedRecipeType = RecipeType.tools;
                    break;
                case "weapons":
                    selectedRecipeType = RecipeType.weapons;
                    break;
            }
            UpdateRecipeUI();
        }
    }
}
