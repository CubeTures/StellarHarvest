using UnityEngine;
using System;

namespace Sunspore
{
    [CreateAssetMenu(fileName = "Store Recipe", menuName = "Scriptable Objects/Store Recipe")]
    public class StoreRecipeSO : ScriptableObject
    {
        public string recipeName;
        public ItemTypeAndAmount[] ingredients;
        public int creditReward = 0;
    }
}
