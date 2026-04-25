using UnityEngine;
using System.Collections;

namespace Sunspore
{
    public class PitSnapper : Plant
    {
        private enum SnapperState
        {
            Idle,
            Attacking,
            Cooldown
        }

        [Header("Pit Snapper Settings")]
        [SerializeField] private float juvenileAbilityPower = 100f;
        [SerializeField] private float fullyGrownAbilityPower = 200f;

        [SerializeField] private float juvenileAbilityRange = 0.5f;
        [SerializeField] private float fullyGrownAbilityRange = 1f;

        [SerializeField] private float juvenileCooldown = 30f;
        [SerializeField] private float fullyGrownCooldown = 20f;

        private bool isAttacking = false;

        private SnapperState currentState = SnapperState.Idle;
        private Animator _animator;

        protected override void Start()
        {
            base.Start();
            _animator = GetComponent<Animator>();
        }

        protected override void UpdateJuvenileAbilities()
        {
            base.UpdateJuvenileAbilities();
            HandleState(juvenileAbilityRange, juvenileAbilityPower, juvenileCooldown);
        }

        protected override void UpdateFullyGrownAbilities()
        {
            base.UpdateFullyGrownAbilities();
            HandleState(fullyGrownAbilityRange, fullyGrownAbilityPower, fullyGrownCooldown);
        }

        private void HandleState(float range, float power, float cooldown)
        {
            switch (currentState)
            {
                case SnapperState.Idle:
                    HandleIdle(range, power, cooldown);
                    break;

                case SnapperState.Attacking:
                    // Do nothing here; handled by coroutine
                    break;

                case SnapperState.Cooldown:
                    // Do nothing here; handled by coroutine
                    break;
            }
        }

        private void HandleIdle(float range, float power, float cooldown)
        {
            if (pathfinding == null || isAttacking) return;

            Enemy enemy = pathfinding.GetEnemyTarget(this, range) as Enemy;

            if (enemy != null)
            {
                StartCoroutine(AttackRoutine(enemy, power, cooldown));
            }
        }

        private IEnumerator AttackRoutine(Enemy enemy, float power, float cooldown)
        {
            isAttacking = true;
            currentState = SnapperState.Attacking;

            _animator.ResetTrigger("AttackLoop");
            _animator.SetTrigger("AttackLoop");

            SFXManager.PlayLoop("pitsnappermunch");

            if (enemy != null)
            {
                enemy.TakeDamage((int)power, Vector2.zero, 0f);
            }

            currentState = SnapperState.Cooldown;
            yield return new WaitForSeconds(cooldown);

            _animator.ResetTrigger("StopLoop");
            _animator.SetTrigger("StopLoop");

            SFXManager.StopLoop("pitsnappermunch");

            currentState = SnapperState.Idle;
            isAttacking = false;
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