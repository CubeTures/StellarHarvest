using UnityEngine;

 [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
 public class ItemSO : ScriptableObject
{
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;
    public bool stackable;
    public int maxStackSize;
    public bool placeable;
    public GameObject prefab;
    
    public int sellPrice;

    public ItemCategory category = ItemCategory.All;
}

public enum ItemCategory { All, Weapons, Seeds, Plants, Tools }