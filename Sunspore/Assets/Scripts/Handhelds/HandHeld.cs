using UnityEngine;

namespace Sunspore
{
    public abstract class HandHeld : MonoBehaviour
    {
        public string handHeldName;
        public int damage;
        public float attackCooldown;
        public float attackDuration;
        public float abilityDuration;
        public float attackRange;
        public float abilityRange;
        public float knockback;
        public Sprite handHeldSprite;
        public Animation attackAnimation;
        public bool melee;
        public bool ranged;
        public bool tool;
        public bool cancelable;

        public virtual void Attack(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mouseDir, Vector3 Playerpos)
        {
            Collider[] hitEnemies = Physics.OverlapBox(
               boxCenter,
               hitArea / 2f,
               boxRotation
           );

            foreach (var enemyCollider in hitEnemies)
            {
                var enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Vector3 direction = (enemy.transform.position - boxCenter).normalized;
                    Vector2 dir = new Vector2(direction.x, direction.z);
                    enemy.TakeDamage(damage, dir, knockback);
                }
            }
        }

        public virtual void Ability(Vector3 Playerpos)
        {

        }
    }
}
