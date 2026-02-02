using UnityEngine;

 [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
 public class ItemSO : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public bool stackable;
    public int maxStackSize;
    public GameObject prefab;


}