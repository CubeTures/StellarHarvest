using UnityEngine;

namespace Sunspore
{
    public class Scythe : HandHeld
    {
        public override bool TryGetAbilityPreview(Player player, Ray ray, out Vector3 previewCenter, out Vector3 previewSize, out bool canUse)
        {
            previewCenter = default;
            previewSize = default;
            canUse = false;

            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                return false;
            }

            var plant = hit.collider.GetComponentInParent<Plant>();
            if (plant != null)
            {
                previewCenter = plant.transform.position;
                previewSize = GetPlantBoundsSize(plant);
                bool inRange = Vector3.Distance(player.transform.position, plant.transform.position) <= abilityRange;
                canUse = inRange && plant.GetGrowthStage() == GrowthStage.FullyGrown;
                return true;
            }

            return false;
        }

        private static Vector3 GetPlantBoundsSize(Plant plant)
        {
            if (plant == null)
            {
                return new Vector3(0.8f, 0f, 0.8f);
            }

            Collider[] colliders = plant.GetComponentsInChildren<Collider>();
            if (colliders == null || colliders.Length == 0)
            {
                return new Vector3(0.8f, 0f, 0.8f);
            }

            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return new Vector3(Mathf.Max(0.8f, bounds.size.x), 0f, Mathf.Max(0.8f, bounds.size.z));
        }

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
