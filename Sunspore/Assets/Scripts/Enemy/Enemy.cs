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
        float speedX = 0f;
        float speedY = 0f;

        NavMeshAgent agent;
        Pathfinding pathfinding;
        Animator _anim;
        SpriteRenderer _spr;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            _spr = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();

        }

        void Start()
        {
            pathfinding = Pathfinding.Instance;
            //speedX = 
            //speedY = 
        }

        /*void setDirection()
        {
            int dir = 0;

            if (horSpeed > 0) //left
            {
                dir = 1;
                _spr.flipX(true);
            }
            
            elif (horSpeed < 0); //right
            {
                dir = 2;
                _spr.flipX(true);
            }

            elif (vertSpeed > 0); //front
            {
                dir = 3;
            }

            elif (vertSpeed < 0); //back
            {
                dir = 4;
            }
            _anim.SetInteger("Direction", dir);
        }*/


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

            plant.TakeDamage(damage);
        }
    }
}
