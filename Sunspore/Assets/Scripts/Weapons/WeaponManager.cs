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
        
        public bool equipped = false;
        private float lastAttackTime = -Mathf.Infinity;
        private Vector3 hitArea;

        private Weapon weapon;

        private void Start()
        {
            hitArea = new Vector3(0.5f, 0.5f, range);
        }

        private void Update()
        {
            if (equipped && Input.GetKeyDown(KeyCode.Space) && Time.time >= lastAttackTime + attackSpeed)
            {
                SetWeaponStats();
                Attack();
                lastAttackTime = Time.time;
            }
        }

        private void Attack()
        {
            // Find all enemies in range
            Collider[] hitEnemies = Physics.OverlapBox(
                transform.position,
                hitArea * 0.5f,
                transform.rotation
            );

            foreach (var enemyCollider in hitEnemies)
            {
                var enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Vector3 direction = (enemy.transform.position - transform.position).normalized;
                    enemy.TakeDamage(damage, direction, knockback);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, hitArea);
        }

        private void SetWeaponStats()
        {

        }
    }

    public abstract class Weapon
    {
        public string name;
        public int damage;
        public float attackSpeed;
        public float attackDuration;
        public float range;
        public float radius;
        public float knockback;
        public Sprite weaponSprite;
    }
}
