using UnityEngine;

namespace Sunspore
{
    [CreateAssetMenu(fileName = "ItemRecipe", menuName = "Scriptable Objects/ItemRecipe")]
    public class ItemRecipeSO : ScriptableObject
    {
        public string recipeName;
        public RecipeType recipeType;
        public ItemTypeAndAmount[] ingredients;
        public ItemTypeAndAmount[] resultItem;
    } 

    public enum RecipeType {tools, weapons};

    [System.Serializable]
    public class ItemTypeAndAmount
    {
        public InventoryItem item;
        public int amount;

        public ItemTypeAndAmount(InventoryItem item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }
}
