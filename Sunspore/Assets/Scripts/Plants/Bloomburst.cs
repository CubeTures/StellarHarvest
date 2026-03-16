using UnityEngine;

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
        [SerializeField] private GameObject _homingBulletPrefab;

        private float _juvenileAbilityTimer = 0f;
        private float _fullyGrownAbilityTimer = 0f;
        private Animator _animator;
        protected override void Start()
        {
            base.Start();
            _animator = GetComponent<Animator>();
            // Debug.Log("Bloom Burst created!");
        }   

        void Update()
        {
            Debug.Log(transitionFinish);
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

            // Shoot at the closest enemy only
            if (closestEnemy != null)
            {
                if (_animator != null)
                    _animator.SetTrigger("JuvenileAttack");

                for (int i = 0; i < juvenileAbilityPower; i++)
                {
                    Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f);
                    GameObject bullet = Instantiate(_homingBulletPrefab, transform.position + offset, Quaternion.identity);
                    bullet.GetComponent<HomingBullet>().SetTarget(closestEnemy);
                }
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

            // Shoot at the closest enemy only
            if (closestEnemy != null)
            {
                if (_animator != null)
                    _animator.SetTrigger("FullyGrownAttack");

                for (int i = 0; i < fullyGrownAbilityPower; i++)
                {
                    Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f);
                    GameObject bullet = Instantiate(_homingBulletPrefab, transform.position + offset, Quaternion.identity);
                    bullet.GetComponent<HomingBullet>().SetTarget(closestEnemy);
                }
            }
        }

        protected override void Harvest()
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
