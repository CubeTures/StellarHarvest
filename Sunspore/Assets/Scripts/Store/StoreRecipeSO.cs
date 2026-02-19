using UnityEngine;
using System;

namespace Sunspore
{
    [CreateAssetMenu(fileName = "Store Recipe", menuName = "Scriptable Objects/Store Recipe")]
    public class StoreRecipeSO : ScriptableObject
    {
        public string recipeName;
        public StoreRecipeType recipeType;
        public ItemTypeAndAmount[] ingredients;
        public ItemTypeAndAmount[] resultItem;
        [TextArea]
        public string description;
        // For buy recipes: credits required to purchase
        public int creditCost = 0;
        public int creditReward = 0;
    }

    public enum StoreRecipeType { buy, sell }
}
