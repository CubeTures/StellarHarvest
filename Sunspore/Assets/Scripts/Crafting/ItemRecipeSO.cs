using UnityEngine;

namespace Sunspore
{
    [CreateAssetMenu(fileName = "Item Recipe", menuName = "Scriptable Objects/Item Recipe")]
    public class ItemRecipeSO : ScriptableObject
    {
        public string recipeName;
        [TextArea]
        public string description;
        public RecipeType recipeType;
        public ItemTypeAndAmount[] ingredients;
        public ItemTypeAndAmount[] resultItem;
    } 

    public enum RecipeType {all, tools, weapons};

    [System.Serializable]
    public class ItemTypeAndAmount
    {
        public ItemSO item;
        public int amount;

        public ItemTypeAndAmount(ItemSO i, int a)
        {
            item = i;
            amount = a;
        }
    }
}
