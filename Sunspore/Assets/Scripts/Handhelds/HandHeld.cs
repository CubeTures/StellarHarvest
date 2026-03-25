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
        public bool weapon;
        public bool tool;
        public bool cancelable;
        public enum WeaponHead { METAL = 1 };
        public enum WeaponHandle { WOOD = 1, METAL = 2 };

        public WeaponHead head;
        public WeaponHandle handle;

        public virtual void Attack(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mousePos, Vector3 Playerpos)
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

        public virtual void Ability(Player player, Ray ray)
        {

        }

        public virtual void AttackWithAbility(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mousePos, Vector3 Playerpos)
        {

        }

        public virtual void CancelAbility()
        {

        }

        public virtual bool CheckRange(Player player, Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log("hit");
                Plant plant = hit.collider.GetComponent<Plant>();
                if (plant != null)
                {
                    float distance = Vector3.Distance(player.transform.position, plant.transform.position);
                    if (distance <= abilityRange)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
