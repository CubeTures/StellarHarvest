using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Sunspore
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] int health = 100;
        [SerializeField] int damage = 10;
        [SerializeField] public int cost = 1;
        [SerializeField] float attackDuration = 0.25f;
        [SerializeField] float attackCooldown = 1.0f;
        private float lastAttackTime = -Mathf.Infinity;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        float eatDelay = 1f;
        float eatCooldown = 0;
        int _currentDirection = -1;
        float _vx;
        float _vz;
        Animator _anim;
        NavMeshAgent agent;
        Pathfinding pathfinding;
        bool attackingPlayer = false;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
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
            if (pathfinding == null)
            {
                pathfinding = Pathfinding.Instance;
            }

            Transform targetPlant = null;
            if (pathfinding != null)
            {
                targetPlant = pathfinding.GetNextTarget(transform, EnemyType.SIMPLE);
            }

            if (agent != null && agent.enabled && agent.isOnNavMesh && targetPlant != null)
            {
                agent.SetDestination(targetPlant.position);
                _vx = targetPlant.position.x - transform.position.x;
                _vz = targetPlant.position.z - transform.position.z;
                SetDirection(_currentDirection);
            }

            if (targetPlant == pathfinding.getPlayerTransform())
            {
                //gameObject.layer = 0;
                attackingPlayer = true;
            }
            else
            {
                //gameObject.layer = LayerMask.NameToLayer("NonPlayerCollide");
                attackingPlayer = false;
            }
            eatCooldown = Mathf.Max(eatCooldown - Time.deltaTime, 0);
        }

        public void SetDirection(int dir)
        {
            _currentDirection = dir;
            _anim.SetInteger("Direction", dir);
            _anim.SetBool("isMoving", true);
            if (_vx > 0)
            {
                _spriteRenderer.flipX = true;
            }
            else if (_vx < 0)
            {
                _spriteRenderer.flipX = false;
            }
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
            if (other.gameObject.GetComponent<Player>() is Player player)
            {
                if (Time.time - lastAttackTime >= attackCooldown && attackingPlayer)
                {
                    Attack(player);
                    lastAttackTime = Time.time;
                }
            }

            void Eat(Plant plant)
            {
                // TODO different things depending on health and type etc
                // maybe take damage as well

                // may need to be a coroutine or some other loop
                StartCoroutine(AttackCoroutine());
                plant.TakeDamage(damage);
            }

            void Attack(Player player)
            {
                StartCoroutine(AttackCoroutine());
                player.TakeDamage(damage);
            }
        }

        // Attack coroutine
        IEnumerator AttackCoroutine()
        {
            _anim.SetBool("isAttacking", true);
            yield return new WaitForSeconds(attackDuration);
            _anim.SetBool("isAttacking", false);
        }
    }
}