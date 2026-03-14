using UnityEngine;
using UnityEngine.EventSystems;
using static Codice.Client.BaseCommands.BranchExplorer.Layout.BrExLayout;
namespace Sunspore
{
    public class WeaponManager : MonoBehaviour
    {
        public static WeaponManager Instance { get; private set; }
        [SerializeField] string weaponName = "default";
        [SerializeField] int damage = 25;
        [SerializeField] float attackCooldown = 0.5f;
        [SerializeField] float attackDuration = 0.2f;
        [SerializeField] float range = 2f;
        [SerializeField] float knockback = 0f;
        [SerializeField] GameObject player;
        [SerializeField] public Sprite weaponSprite;
        [SerializeField] public Animation attackAnimation;
        [SerializeField] public bool melee = false;
        [SerializeField] public bool ranged = false;
        public new bool enabled = false;
        private float lastAttackTime = -Mathf.Infinity;
        private Vector3 hitArea;
        private Quaternion boxRotation;
        private Vector3 boxCenter;
        private float offset;
        private Vector3[] directions = new Vector3[4];
        private Vector3 currDirection = Vector3.zero;
        
        [SerializeField] private GameObject attackBoxPrefab;
        private GameObject attackBox;
        private Weapon weapon;
        private Player playerScript;
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
            directions[0] = new Vector3(0.0f, 0.0f, -1.0f);
            directions[1] = new Vector3(1.0f, 0.0f, 0.0f);
            directions[2] = new Vector3(0.0f, 0.0f, 1.0f);
            directions[3] = new Vector3(-1.0f, 0.0f, 0.0f);
            offset = range / 2;
            player = GameObject.FindGameObjectWithTag("Player");
            playerScript = player.GetComponent<Player>();
            attackBox = Instantiate(attackBoxPrefab, transform);
            attackBox.transform.localScale = hitArea * 0.5f;
            attackBox.SetActive(false);
        }
        private void Update()
        {
            if (enabled && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
            {
                Vector3 mouseWorldPos = GetMouseWorldPosition();
                Attack(mouseWorldPos);
                lastAttackTime = Time.time;
            }
            attackBox.SetActive(enabled);
            if (enabled)
            {
                updateDirection();
            }
        }
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
            {
                return hit.point;
            }
            else
            {
                float t = -ray.origin.y / ray.direction.y;
                return ray.origin + ray.direction * t;
            }
        }
        private void Attack(Vector3 mouseWorldPos)
        {
            // Find all enemies in range
            updateDirection();
            Physics.SyncTransforms();
            if (attackBox != null)
            {
                var renderer = attackBox.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = PreserveAlpha(renderer.material.color, Color.red);
                }
                
                Invoke(nameof(StopAttacking), attackDuration);
            }
            Vector3 playerPos = player.transform.position;
            mouseWorldPos.y = 1.5f;
            Vector3 mouseDir = (mouseWorldPos - playerPos).normalized;
            weapon.Attack(boxCenter, hitArea, boxRotation, mouseDir, playerPos);
            playerScript.Swing(SwingDirection.WeaponType.METAL);
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
                renderer.material.color = PreserveAlpha(renderer.material.color, Color.gray);
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
        public void SetWeaponStats(GameObject weaponPrefab)
        {
            weapon = weaponPrefab.GetComponent<Weapon>();
            weaponName = weapon.weaponName;
            damage = weapon.damage;
            attackCooldown = weapon.attackCooldown;
            attackDuration = weapon.attackDuration;
            range = weapon.range;
            knockback = weapon.knockback;
            weaponSprite = weapon.weaponSprite;
            attackAnimation = weapon.attackAnimation;
            melee = weapon.melee;
            ranged = weapon.ranged;
            //if (!melee)
            //{
            //    attackBox.SetActive(false);
            //}
            //else
            //{
            //    attackBox.SetActive(true);
            //}
        }
        private Color PreserveAlpha(Color original, Color next)
        {
            return new Color(next.r, next.g, next.b, original.a);
        }
    }
}