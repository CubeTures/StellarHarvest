using UnityEngine;

namespace Sunspore
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
    public class Weapon : ScriptableObject
    {
        public string weaponName;
        public int damage;
        public float attackSpeed;
        public float attackDuration;
        public float range;
        public float radius;
        public float knockback;
        public Sprite weaponSprite;
        public Animation attackAnimation;
    }
}
