using UnityEngine;
using static Codice.Client.BaseCommands.BranchExplorer.Layout.BrExLayout;

namespace Sunspore
{
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] string waponName = "default";
        [SerializeField] int damage = 25;
        [SerializeField] float attackSpeed = 0.5f;
        [SerializeField] float attackDuration = 1;
        [SerializeField] float range = 10f;
        [SerializeField] float radius = 100f;
        [SerializeField] float knockback = 0f;
        [SerializeField] GameObject player;
        
        public bool enabled = false;
        private float lastAttackTime = -Mathf.Infinity;
        private Vector3 hitArea;
        private Vector3[] directions = new Vector3[4];
        private Vector3 currDirection = Vector3.zero;

        private Weapon weapon;

        private void Start()
        {
            hitArea = new Vector3(0.5f, 0.5f, range);
            directions[0] = new Vector3(0.0f, 0.0f, 1.0f);
            directions[1] = new Vector3(1.0f, 0.0f, 0.0f);
            directions[2] = new Vector3(0.0f, 0.0f, -1.0f);
            directions[3] = new Vector3(-1.0f, 0.0f, 0.0f);

            player = GameObject.FindGameObjectWithTag("Player");
        }

        private void Update()
        {
            if (enabled && Input.GetKeyDown(KeyCode.Space) && Time.time >= lastAttackTime + attackSpeed)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }

        private void Attack()
        {
            Debug.Log(currDirection);
            // Find all enemies in range
            Quaternion boxRotation = currDirection != Vector3.zero
                ? Quaternion.LookRotation(currDirection)
                : player.transform.rotation;

            Collider[] hitEnemies = Physics.OverlapBox(
                player.transform.position,
                hitArea * 0.5f,
                boxRotation
            );

            foreach (var enemyCollider in hitEnemies)
            {
                var enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Vector3 direction = (enemy.transform.position - player.transform.position).normalized;
                    enemy.TakeDamage(damage, direction, knockback);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Quaternion boxRotation = currDirection != Vector3.zero
                ? Quaternion.LookRotation(currDirection)
                : player.transform.rotation;

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(player.transform.position, boxRotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, hitArea);
        }

        public void setDirection(int dir)
        {
            currDirection = directions[dir];
        }

        public void SetWeaponStats()
        {

        }
    }
}
