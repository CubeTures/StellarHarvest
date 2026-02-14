using UnityEngine;
using UnityEngine.EventSystems;
using static Codice.Client.BaseCommands.BranchExplorer.Layout.BrExLayout;

namespace Sunspore
{
    public class WeaponManager : MonoBehaviour
    {
        public static WeaponManager Instance { get; private set; }

        [SerializeField] string waponName = "default";
        [SerializeField] int damage = 25;
        [SerializeField] float attackCooldown = 0.5f;
        [SerializeField] float attackDuration = 0.2f;
        [SerializeField] float range = 2.5f;
        [SerializeField] float knockback = 0f;
        [SerializeField] GameObject player;

        public bool enabled = false;

        private float lastAttackTime = -Mathf.Infinity;
        private Vector3 hitArea;
        private float offset;

        private Vector3[] directions = new Vector3[4];
        private Vector3 currDirection = Vector3.zero;

        private Quaternion boxRotation;
        private Vector3 boxCenter;
        private GameObject attackBox = null;

        private Weapon weapon;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }


        private void Start()
        {
            hitArea = new Vector3(1.0f, 1.0f, range);
            directions[0] = new Vector3(1.0f, 0.0f, 0.0f);
            directions[1] = new Vector3(-1.0f, 0.0f, 0.0f);
            directions[2] = new Vector3(0.0f, 0.0f, -1.0f);
            directions[3] = new Vector3(0.0f, 0.0f, 1.0f);
            offset = range / 2;

            player = GameObject.FindGameObjectWithTag("Player");

            if (attackBox == null)
            {
                attackBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(attackBox.GetComponent<Collider>());
            }

            if (attackBox != null)
            {
                attackBox.transform.localScale = hitArea;
                attackBox.SetActive(false);
            }
        }

        private void Update()
        {
            if (enabled && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }

            attackBox.SetActive(enabled);

            if (enabled)
            {
                updateDirection();
            }
        }

        private void Attack()
        {
            //Debug.Log(currDirection);

            // Find all enemies in range
            updateDirection();

            if (attackBox != null)
            {
                var renderer = attackBox.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
                
                Invoke(nameof(StopAttacking), attackDuration);
            }

            Collider[] hitEnemies = Physics.OverlapBox(
                boxCenter,
                hitArea,
                boxRotation
            );

            foreach (var enemyCollider in hitEnemies)
            {
                var enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Vector3 direction = (enemy.transform.position - boxCenter).normalized;
                    enemy.TakeDamage(damage, direction, knockback);
                }
            }
        }

        //private void OnDrawGizmos()
        //{
        //    Quaternion boxRotation = currDirection != Vector3.zero
        //        ? Quaternion.LookRotation(currDirection)
        //        : player.transform.rotation;

        //    Gizmos.color = Color.red;
        //    Gizmos.matrix = Matrix4x4.TRS(boxCenter, boxRotation, Vector3.one);
        //    Gizmos.DrawWireCube(Vector3.zero, hitArea * 1.5f);
        //}

        private void StopAttacking()
        {
            var renderer = attackBox.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.gray;
            }
        }

        public void setDirection(int dir)
        {
            currDirection = directions[dir-1];
        }

        private void updateDirection()
        {
            boxRotation = currDirection != Vector3.zero
                ? Quaternion.LookRotation(currDirection)
                : player.transform.rotation;

            boxCenter = player.transform.position + currDirection.normalized * offset;

            if (attackBox != null)
            {
                attackBox.transform.position = boxCenter;
                attackBox.transform.rotation = boxRotation;
                attackBox.transform.localScale = hitArea;
                //Debug.Log("hi");
            }
        }

        public void SetWeaponStats()
        {

        }
    }
}
