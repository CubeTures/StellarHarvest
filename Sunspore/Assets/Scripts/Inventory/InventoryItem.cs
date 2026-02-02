using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class InventoryItem : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public bool stackable;
    public bool placeable;
    public GameObject prefab;
    public bool weapon;

    public override bool Equals(object obj)
    {
        if (obj is not InventoryItem other) return false;
        return id == other.id;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }
}