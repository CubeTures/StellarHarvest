using UnityEngine;
using UnityEngine.Tilemaps;

namespace Sunspore
{
    /// <summary>
    /// Bloom Burst plant that shoots homing bullets at enemies
    /// </summary>
    public class Bloomburst : Plant
    {
        [Header("Bloom Burst Specific Settings")]
        [SerializeField] private float juvenileAbilityPower = 1f;
        [SerializeField] private float fullyGrownAbilityPower = 3f;
        [SerializeField] private float juvenileAbilityRange = 10f;
        [SerializeField] private float fullyGrownAbilityRange = 10f;
        [SerializeField] private float _juvenileAbilityCooldown = 3f;
        [SerializeField] private float _fullyGrownAbilityCooldown = 2f;
        [SerializeField] private float bulletDelay = 1f;
        [SerializeField] private GameObject _homingBulletPrefab;

        [Header("Placement")]
        [SerializeField] private bool snapToGridOnStart = false;
        [SerializeField] private float gridSizeX = 1.5f;
        [SerializeField] private float gridSizeZ = 2.25f;
        [SerializeField] private bool tillGroundOnStart = false;

        private float _juvenileAbilityTimer = 0f;
        private float _fullyGrownAbilityTimer = 0f;
        private Animator _animator;
        protected override void Start()
        {
            if (snapToGridOnStart && IsTutorialActive())
            {
                SnapToGrid();
            }

            if (
                tillGroundOnStart
                && IsTutorialActive()
                && TerrainTilemap.Instance != null
            )
            {
                TerrainTilemap.Instance.TillAtWorldPosition(transform.position, 0f, transform.position);
            }

            base.Start();
            _animator = GetComponent<Animator>();
            // Debug.Log("Bloom Burst created!");
        }

        [ContextMenu("Snap To Placement Grid")]
        private void SnapToGrid()
        {
            Tilemap tilemap = TerrainTilemap.Instance != null ? TerrainTilemap.Instance.GetComponent<Tilemap>() : null;
            if (tilemap != null)
            {
                Vector3Int cell = tilemap.WorldToCell(transform.position);
                Vector3 center = tilemap.GetCellCenterWorld(cell);
                transform.position = new Vector3(center.x, transform.position.y, center.z);
                return;
            }

            if (gridSizeX <= 0f || gridSizeZ <= 0f)
            {
                return;
            }

            Vector3 pos = transform.position;
            float snappedX = Mathf.Round(pos.x / gridSizeX) * gridSizeX;
            float snappedZ = Mathf.Round(pos.z / gridSizeZ) * gridSizeZ;
            transform.position = new Vector3(snappedX, pos.y, snappedZ);
        }

        private static bool IsTutorialActive()
        {
            return TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive;
        }

        protected override void OnSeedStage()
        {
            base.OnSeedStage();
            // Debug.Log("Plant is now a seed - no abilities");
        }

        protected override void OnJuvenileStage()
        {
            base.OnJuvenileStage();
            // Debug.Log("Plant is now juvenile - limited abilities active");
        }

        protected override void OnFullyGrownStage()
        {
            base.OnFullyGrownStage();
            // Debug.Log("Plant is now fully grown - full abilities active");
        }

        protected override void UpdateJuvenileAbilities()
        {
            base.UpdateJuvenileAbilities();

            _juvenileAbilityTimer += Time.deltaTime;
            if (_juvenileAbilityTimer >= _juvenileAbilityCooldown)
            {
                UseJuvenileAbility();
                _juvenileAbilityTimer = 0f;
            }
        }

        protected override void UpdateFullyGrownAbilities()
        {
            base.UpdateFullyGrownAbilities();

            _fullyGrownAbilityTimer += Time.deltaTime;
            if (_fullyGrownAbilityTimer >= _fullyGrownAbilityCooldown)
            {
                UseFullAbility();
                _fullyGrownAbilityTimer = 0f;
            }
        }

        private void UseJuvenileAbility()
        {
            // Debug.Log($"Juvenile ability activated with power: {juvenileAbilityPower}");

            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, juvenileAbilityRange);
            Transform closestEnemy = null;
            float closestDistance = float.MaxValue;

            // Find the closest enemy
            foreach (Collider col in nearbyObjects)
            {
                if (col.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = col.transform;
                    }
                }
            }

            // Shoot at the closest enemy only, with a delay between bullets
            if (closestEnemy != null)
            {
                if (_animator != null)
                    _animator.SetTrigger("Attack");

                StartCoroutine(SpawnBulletsOverTime(closestEnemy, juvenileAbilityPower));
            }
        }

        private void UseFullAbility()
        {
            // Debug.Log($"Full ability activated with power: {fullyGrownAbilityPower}");

            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, fullyGrownAbilityRange);
            Transform closestEnemy = null;
            float closestDistance = float.MaxValue;

            // Find the closest enemy
            foreach (Collider col in nearbyObjects)
            {
                if (col.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = col.transform;
                    }
                }
            }

            // Shoot at the closest enemy only, with a delay between bullets
            if (closestEnemy != null)
            {
                if (_animator != null)
                    _animator.SetTrigger("Attack");

                StartCoroutine(SpawnBulletsOverTime(closestEnemy, fullyGrownAbilityPower));
            }
        }

        private System.Collections.IEnumerator SpawnBulletsOverTime(Transform target, float bulletCount)
        {
            // Small fixed delay between bullets; tweak as needed
            float delayBetweenBullets = bulletDelay;

            int count = Mathf.RoundToInt(bulletCount);
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0f
                );
                GameObject bullet = Instantiate(
                    _homingBulletPrefab,
                    transform.position + offset,
                    Quaternion.identity
                );
                PlaySound(attackSound);

                var homing = bullet.GetComponent<HomingBullet>();
                if (homing != null)
                {
                    homing.SetTarget(target);
                }

                yield return new WaitForSeconds(delayBetweenBullets);
            }
        }

        public override void Harvest()
        {
            base.Harvest();
            // Debug.Log("Bloom Burst harvested!");
        }

        private void OnDrawGizmosSelected()
        {
            if (currentStage == GrowthStage.Juvenile)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, juvenileAbilityRange);
            }
            else if (currentStage == GrowthStage.FullyGrown)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, fullyGrownAbilityRange);
            }
        }
    }
}
