using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Sunspore
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] public int health = 100;
        [SerializeField] public int maxHealth = 100;
        [SerializeField] public EnemyType enemyType = EnemyType.SWARM;
        public FlyingState flyingState = FlyingState.AIRBORNE;
        [SerializeField] int damage = 10;
        [SerializeField] public float cost = 1f;

        [Header("Spawning")]
        [SerializeField, Range(0f, 1f)] public float spawnChance = 0.25f;
        [SerializeField, Range(0f, 1f)] public float groupSpawnChance = 0.3f;
        [SerializeField] public int minGroupSize = 2;
        [SerializeField] public int maxGroupSize = 4;
        [SerializeField] public float groupSpread = 2f;
        [SerializeField] public float groupSizeScale = 0.5f;

        [SerializeField] float attackDuration = 0.25f;
        [SerializeField] float attackCooldown = 1.0f;
        [SerializeField] float flyHeight = 3f;
        [SerializeField] float perchRadius = 2f;
        [SerializeField] float descentSpeed = 3f;
        private float lastAttackTime = -Mathf.Infinity;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        float eatDelay = 1f;
        float eatCooldown = 0;
        int _currentDirection = -1;
        float _vx;
        float _vz;
        Animator _anim;
        public NavMeshAgent agent;
        Pathfinding pathfinding;
        bool attackingPlayer = false;
        bool isAttacking = false;
        [SerializeField] private GameObject _hitEffect;
        [SerializeField] private GameObject _deathEffect;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
            if (_spriteRenderer != null)
                _originalColor = _spriteRenderer.color;
        }

        void Start()
        {
            pathfinding = Pathfinding.Instance;
            pathfinding.RegisterEnemy(this);
        }

        public void TakeDamage(int weaponDamage, Vector2 direction, float knockback)
        {
            health -= weaponDamage;
            Instantiate(_hitEffect, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.red;
                CancelInvoke(nameof(ResetColor));
                Invoke(nameof(ResetColor), 0.2f); // 0.2 seconds, adjust as needed
            }
            if (health <= 0)
            {
                Instantiate(_deathEffect, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
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
                pathfinding = Pathfinding.Instance;

            Transform targetPlant = null;
            if (pathfinding != null)
                targetPlant = pathfinding.GetNextTarget(this);

            if (agent != null && agent.enabled && agent.isOnNavMesh && targetPlant != null)
            {
                float distToTarget = Vector3.Distance(transform.position, targetPlant.position);
                if (distToTarget > agent.stoppingDistance)
                {
                    agent.isStopped = false;
                    agent.SetDestination(targetPlant.position);
                }
                else
                {
                    agent.isStopped = true;
                    agent.SetDestination(transform.position);
                }

                _vx = targetPlant.position.x - transform.position.x;
                _vz = targetPlant.position.z - transform.position.z;
                SetDirection(_currentDirection);

                if (enemyType == EnemyType.FLYING)
                    UpdateFlyingState(targetPlant);
            }

            // update animator movement state (disable while attacking)
            if (_anim != null)
            {
                bool moving = !isAttacking && agent != null && agent.velocity.sqrMagnitude > 0.01f;
                _anim.SetBool("isMoving", moving);
            }

            attackingPlayer = targetPlant == pathfinding.GetPlayerTransform();
            eatCooldown = Mathf.Max(eatCooldown - Time.deltaTime, 0);
            _spriteRenderer.sortingOrder = (int)(-transform.position.z * 100);

            updateDirection();
        }

        void UpdateFlyingState(Transform target)
        {
            bool inPerchRange = agent.remainingDistance <= perchRadius && !agent.pathPending;

            if (inPerchRange && flyingState == FlyingState.AIRBORNE)
                flyingState = FlyingState.LANDED;

            if (!inPerchRange && flyingState != FlyingState.AIRBORNE)
            {
                flyingState = FlyingState.AIRBORNE;
                // TODO: restore wall-ignoring area mask here
            }

            // float targetY = flyingState == FlyingState.AIRBORNE ? flyHeight : 0f;
            // float newY = Mathf.MoveTowards(transform.position.y, targetY, descentSpeed * Time.deltaTime);
            // transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // if (flyingState == FlyingState.LANDING && Mathf.Approximately(transform.position.y, 0f))
            //     flyingState = FlyingState.LANDED;
        }
        void updateDirection()
        {
            if (Mathf.Abs(_vx) > Mathf.Abs(_vz))
            {
                // Horizontal wins
                if (_vx > 0)
                {
                    SetDirection(2); // Right
                }
                else
                {
                    SetDirection(4); // Left
                }
            }
            else
            {
                // Vertical wins
                if (_vz > 0)
                {
                    SetDirection(3); // Forward
                }
                else
                {
                    SetDirection(1); // Back            
                }
            }
        }
        public void SetDirection(int dir)
        {
            _currentDirection = dir;
            _anim.SetInteger("Direction", dir);

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
            _anim.SetTrigger("Attack");
            // mark attacking, stop movement while attack animation runs
            isAttacking = true;
            if (_anim != null)
                _anim.SetBool("isMoving", false);
            if (agent != null)
                agent.isStopped = true;

            yield return new WaitForSeconds(attackDuration);
            _anim.ResetTrigger("Attack");

            // restore movement
            isAttacking = false;
            if (agent != null)
                agent.isStopped = false;
            if (_anim != null)
                _anim.SetBool("isMoving", true);
        }

        void OnDestroy()
        {
            pathfinding.UnregisterEnemy(this);
        }
    }
}