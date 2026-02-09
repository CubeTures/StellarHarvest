using UnityEngine;
using UnityEngine.AI;

namespace Sunspore
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] int health = 100;
        [SerializeField] int damage = 10;
        [SerializeField] Player player;
        float eatDelay = 1f;
        float eatCooldown = 0;

        NavMeshAgent agent;
        Pathfinding pathfinding;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            pathfinding = Pathfinding.Instance;
        }

        public void TakeDamage(int weaponDamage, Vector2 direction, float knockback)
        {
            health -= weaponDamage;
            if (health <= 0)
            {
                Destroy(this.gameObject);
            }
        }

        void Update()
        {
            Transform targetPlant = pathfinding.GetNextTarget(transform, EnemyType.SIMPLE);
            agent.SetDestination(targetPlant.position);
            eatCooldown = Mathf.Max(eatCooldown - Time.deltaTime, 0);
        }

        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.GetComponent<Plant>() is Plant plant)
            {
                if (eatCooldown == 0)
                {
                    Eat(plant);
                    eatCooldown = eatDelay;
                }
            }
        }

        void Eat(Plant plant)
        {
            // TODO different things depending on health and type etc
            // maybe take damage as well

            // may need to be a coroutine or some other loop

            plant.TakeDamage(25);
        }
    }
}
