using UnityEngine;

namespace Sunspore
{
    public class Sword : Weapon
    {
        public override void Attack(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mouseDir, Vector3 playerPos)
        {
            Collider[] hitEnemies = Physics.OverlapBox(
                boxCenter,
                hitArea,
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
    }
}
