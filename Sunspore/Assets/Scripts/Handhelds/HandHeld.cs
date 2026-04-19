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
        public enum WeaponHead { SHOVEL = 1, HOE = 2, SCYTHE = 3};
        public enum WeaponHandle { WOOD = 1, METAL = 2 };
        [SerializeField] public Sprite headSprite;

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

        public virtual void AttackWithAbility(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mousePos, Vector3 Playerpos, bool z)
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
                    Vector3 plantDist = plant.transform.position;

                    // Line up plant with player and account for camera shift
                    plantDist.y = 1.5f;
                    plantDist.z /= 1.5f;
                    Vector3 playerPos = player.transform.position;
                    playerPos.z /= 1.5f;
                    float distance = Vector3.Distance(playerPos, plantDist);
                    Debug.Log("Player Position: " + playerPos + " Plant: " + plant.transform.position + " Distance: " + distance);
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
