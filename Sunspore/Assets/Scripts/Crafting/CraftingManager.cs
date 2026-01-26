using UnityEngine;

namespace Sunspore
{
    public class CraftingManager : MonoBehaviour
    {
        public RecipeType selectedRecipeType;

        [SerializeField] ItemRecipeSO[] recipes;
        [SerializeField] GameObject recipeUIPrefab;
        [SerializeField] Transform recipeUIParent;

        private void OnEnable()
        {
            selectedRecipeType = RecipeType.tools;
            UpdateRecipeUI();
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
                    newRecipeUI.name = recipes[i].name;
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
                itemRecipe.UpdateRecipeUI(itemRecipeSO);

            }
        }

        public void SelectedRecipeType(RecipeType type)
        {
            switch (type)
            {
                case RecipeType.tools:
                    selectedRecipeType = RecipeType.tools;
                    break;
                case RecipeType.weapons:
                    selectedRecipeType = RecipeType.weapons;
                    break;
                default:
                    selectedRecipeType = RecipeType.tools;
                    break;
            }

            UpdateRecipeUI();
        }

    }
}
