using UnityEngine;

namespace Sunspore
{
    public abstract class Weapon : MonoBehaviour
    {
        public string weaponName;
        public int damage;
        public float attackCooldown;
        public float attackDuration;
        public float range;
        public float knockback;
        public Sprite weaponSprite;
        public Animation attackAnimation;
        public bool melee;
        public bool ranged;

        public virtual void Attack(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mouseDir, Vector3 Playerpos)
        {
            // Each Weapon Has A Unique Attack
        }
    }
}
