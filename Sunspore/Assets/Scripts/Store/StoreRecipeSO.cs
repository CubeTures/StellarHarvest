using UnityEngine;
using System;

namespace Sunspore
{
    [CreateAssetMenu(fileName = "Store Recipe", menuName = "Scriptable Objects/Store Recipe")]
    public class StoreRecipeSO : ScriptableObject
    {
        public string recipeName;
        public StoreRecipeType recipeType;
        // Category used to filter recipes in the store UI (plants, weapons, tools, etc.)
        public StoreRecipeCategory category = StoreRecipeCategory.All;
        public ItemTypeAndAmount[] ingredients;
        public ItemTypeAndAmount[] resultItem;
        [TextArea]
        public string description;
        // For buy recipes: credits required to purchase
        public int creditCost = 0;
        public int creditReward = 0;
    }

    public enum StoreRecipeType { buy, sell }

    public enum StoreRecipeCategory { All, Plants, Weapons, Tools }
}
