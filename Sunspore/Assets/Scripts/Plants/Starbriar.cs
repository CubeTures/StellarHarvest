using UnityEngine;

namespace Sunspore
{
	public class Starbriar : Plant
	{
		[Header("Starbriar Specific Settings")]
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
			var collider = GetComponent<BoxCollider>();
			NavMeshTerrain.Instance.AddObstacle(gameObject, collider);
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
			// no ability
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

		private void Oestroy()
		{
			NavMeshTerrain.Instance.RemoveObstacle(gameObject);			
		}
	}
}
