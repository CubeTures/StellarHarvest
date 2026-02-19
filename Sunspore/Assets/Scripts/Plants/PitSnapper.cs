using UnityEngine;

namespace Sunspore
{
    /// <summary>
    /// Pit Snapper plant that deals high damage at close range with long cooldowns
    /// </summary>
    public class PitSnapper : Plant
    {
        [Header("Pit Snapper Specific Settings")]
        [SerializeField] private float juvenileAbilityPower = 100f;
        [SerializeField] private float fullyGrownAbilityPower = 200f;
        [SerializeField] private float juvenileAbilityRange = 0.5f;
        [SerializeField] private float fullyGrownAbilityRange = 1f;
        [SerializeField] private float _juvenileAbilityCooldown = 30f;
        [SerializeField] private float _fullyGrownAbilityCooldown = 20f;

        private float _juvenileAbilityTimer = 0f;
        private float _fullyGrownAbilityTimer = 0f;
        private Animator _animator;
        protected override void Start()
        {
            base.Start();
            _animator = GetComponent<Animator>();
            // Debug.Log("Pit Snapper created!");
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
            foreach (Collider col in nearbyObjects)
            {
                if (col.CompareTag("Enemy"))
                {
                    _animator.SetTrigger("JuvenileAttack");

                    col.GetComponent<Enemy>()?.TakeDamage((int)juvenileAbilityPower, Vector2.zero, 0f);
                }
            }
        }

        private void UseFullAbility()
        {
            // Debug.Log($"Full ability activated with power: {fullyGrownAbilityPower}");

            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, fullyGrownAbilityRange);
            foreach (Collider col in nearbyObjects)
            {
                if (col.CompareTag("Enemy"))
                {
                    _animator.SetTrigger("FullyGrownAttack");

                    col.GetComponent<Enemy>()?.TakeDamage((int)fullyGrownAbilityPower, Vector2.zero, 0f);
                }
            }
        }

        protected override void Harvest()
        {
            base.Harvest();
            // Debug.Log("Pit Snapper harvested!");
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
