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
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<Plant>() is Plant plant)
            {
                Eat(plant);
            }
        }

        void Eat(Plant plant)
        {
            // TODO different things depending on health and type etc
            // maybe take damage as well

            // may need to be a coroutine or some other loop

            Destroy(plant.gameObject);
        }
    }
}
