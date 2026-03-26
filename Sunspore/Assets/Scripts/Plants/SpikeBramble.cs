using UnityEngine;

namespace Sunspore
{
    /// <summary>
    /// Spike Bramble plant that damages nearby enemies
    /// </summary>
    public class SpikeBramble : Plant
    {
        [Header("Spike Bramble Specific Settings")]
        [SerializeField] private float juvenileAbilityPower = 5f;
        [SerializeField] private float fullyGrownAbilityPower = 15f;
        [SerializeField] private float juvenileAbilityRange = 1f;
        [SerializeField] private float fullyGrownAbilityRange = 2f;
        [SerializeField] private float _juvenileAbilityCooldown = 2f;
        [SerializeField] private float _fullyGrownAbilityCooldown = 1f;

        private float _juvenileAbilityTimer = 0f;
        private float _fullyGrownAbilityTimer = 0f;
        private Animator _animator;

        protected override void Start()
        {
            base.Start();
            _animator = GetComponent<Animator>();
            // Debug.Log("Spike Bramble created!");
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

            if (pathfinding.GetEnemyTarget(this, juvenileAbilityRange) is Enemy enemy)
            {
                _animator.SetTrigger("FullyGrownAttack");
                enemy.TakeDamage((int)fullyGrownAbilityPower, Vector2.zero, 0f);
            }
        }

        private void UseFullAbility()
        {
            // Debug.Log($"Full ability activated with power: {fullyGrownAbilityPower}");

            if (pathfinding.GetEnemyTarget(this, fullyGrownAbilityRange) is Enemy enemy)
            {
                _animator.SetTrigger("FullyGrownAttack");
                enemy.TakeDamage((int)fullyGrownAbilityPower, Vector2.zero, 0f);
            }
        }

        public override void Harvest()
        {
            base.Harvest();
            // Debug.Log("Spike Bramble harvested!");
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
