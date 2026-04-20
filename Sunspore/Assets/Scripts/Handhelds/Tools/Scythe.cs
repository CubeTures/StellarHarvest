using UnityEngine;

namespace Sunspore
{
    public class Scythe : HandHeld
    {
        public override void Attack(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mousePos, Vector3 playerPos)
        {
            hitArea.x = abilityRange;
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
            SFXManager._sfxManager.PlaySound("swing");
        }


        // Scythe no longer attacks when harvesting
        public override void AttackWithAbility(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mousePos, Vector3 Playerpos, bool z)
        {
            Vector3 harvestArea = hitArea;
            harvestArea.z = abilityRange;
            if (z)
            {
                harvestArea.z *= 1.5f;
            }

            // Destroy grass
            Collider[] hitTargets = Physics.OverlapBox(
                boxCenter,
                hitArea / 2f,
                boxRotation
            );

            //Debug.Log(hitTargets);
            foreach (var collider in hitTargets)
            {
                //var enemy = collider.GetComponent<Enemy>();
                //if (enemy != null)
                //{
                //    Vector3 direction = (enemy.transform.position - boxCenter).normalized;
                //    Vector2 dir = new Vector2(direction.x, direction.z);
                //    enemy.TakeDamage(damage, dir, knockback);
                //}
                var grass = collider.GetComponent<Grass>();
                if(grass != null)
                {
                    grass.DestroyGrass();
                }
            }

            // Harvest plants

            hitTargets = Physics.OverlapBox(
               boxCenter,
               harvestArea / 2f,
               boxRotation
            );

            foreach (var collider in hitTargets)
            {
                var plant = collider.GetComponent<Plant>();
                if (plant != null)
                {
                    plant.Harvest();
                }

            }
        }
    }
}
