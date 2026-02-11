using UnityEngine;
using UnityEngine.AI;

namespace Sunspore
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] int health = 100;
        [SerializeField] int damage = 10;
        [SerializeField] int attackDuration = 1;
        [SerializeField] float attackCooldown = 1.0f;
        private float lastAttackTime = -Mathf.Infinity;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;

        NavMeshAgent agent;
        Pathfinding pathfinding;
        bool attackingPlayer = false;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
                _originalColor = _spriteRenderer.color;
        }

        void Start()
        {
            pathfinding = Pathfinding.Instance;
        }

        public void TakeDamage(int weaponDamage, Vector2 direction, float knockback)
        {
            health -= weaponDamage;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.red;
                CancelInvoke(nameof(ResetColor));
                Invoke(nameof(ResetColor), 0.2f); // 0.2 seconds, adjust as needed
            }
            if (health <= 0)
            {
                Destroy(this.gameObject);
            }
        }

        private void ResetColor()
        {
            if (_spriteRenderer != null)
                _spriteRenderer.color = _originalColor;
        }

        void Update()
        {
            Transform targetPlant = pathfinding.GetNextTarget(transform, EnemyType.SIMPLE);
            agent.SetDestination(targetPlant.position);
            if(targetPlant == pathfinding.getPlayerTransform())
            {
                //gameObject.layer = 0;
                attackingPlayer = true;
            }
            else
            {
                //gameObject.layer = LayerMask.NameToLayer("NonPlayerCollide");
                attackingPlayer = false;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<Plant>() is Plant plant)
            {
                Eat(plant);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.GetComponent<Player>() is Player player)
            {
                if (Time.time - lastAttackTime >= attackCooldown && attackingPlayer)
                {
                    Attack(player);
                    lastAttackTime = Time.time;
                }
            }
        }

        void Eat(Plant plant)
        {
            // TODO different things depending on health and type etc
            // maybe take damage as well

            // may need to be a coroutine or some other loop

            Destroy(plant.gameObject);
        }

        void Attack(Player player)
        {
            player.TakeDamage(damage);
        }
    }
}
