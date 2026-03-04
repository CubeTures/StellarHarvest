using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using static Codice.Client.BaseCommands.BranchExplorer.Layout.BrExLayout;
namespace Sunspore
{
    public class HandHeldManager : MonoBehaviour
    {
<<<<<<< HEAD:Sunspore/Assets/Scripts/Weapons/WeaponManager.cs
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
        public Weapon.WeaponHead head = Weapon.WeaponHead.METAL;
        public Weapon.WeaponHandle handle = Weapon.WeaponHandle.WOOD;
=======
        public static HandHeldManager Instance { get; private set; }

        [SerializeField] GameObject player;

>>>>>>> 2b74cbe (revamped weapons to handhelds for tools and added watering can):Sunspore/Assets/Scripts/Handhelds/HandHeldManager.cs
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
<<<<<<< HEAD:Sunspore/Assets/Scripts/Weapons/WeaponManager.cs
        private Weapon weapon;
        private Player playerScript;
=======

        private HandHeld handHeld;

>>>>>>> 2b74cbe (revamped weapons to handhelds for tools and added watering can):Sunspore/Assets/Scripts/Handhelds/HandHeldManager.cs
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
            // Setting up direction and attackBox

            hitArea = new Vector3(1.0f, 1.0f, 1.0f);
            directions[0] = new Vector3(0.0f, 0.0f, -1.0f);
            directions[1] = new Vector3(1.0f, 0.0f, 0.0f);
            directions[2] = new Vector3(0.0f, 0.0f, 1.0f);
            directions[3] = new Vector3(-1.0f, 0.0f, 0.0f);
<<<<<<< HEAD:Sunspore/Assets/Scripts/Weapons/WeaponManager.cs
            offset = range / 2;
=======
            offset = 1 / 2;

>>>>>>> 2b74cbe (revamped weapons to handhelds for tools and added watering can):Sunspore/Assets/Scripts/Handhelds/HandHeldManager.cs
            player = GameObject.FindGameObjectWithTag("Player");
            playerScript = player.GetComponent<Player>();
            attackBox = Instantiate(attackBoxPrefab, transform);
            //attackBox.transform.localScale = hitArea;
            attackBox.SetActive(false);
        }
        private void Update()
        {
            // If lieft click attack

            if (enabled && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + handHeld.attackCooldown)
            {
                Vector3 mouseWorldPos = GetMouseWorldPosition();
                Attack(mouseWorldPos);
                lastAttackTime = Time.time;
            }
<<<<<<< HEAD:Sunspore/Assets/Scripts/Weapons/WeaponManager.cs
            attackBox.SetActive(enabled);
=======

            // If right click cast ability

            if (enabled && handHeld.tool && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(1) && Time.time >= lastAttackTime + handHeld.attackCooldown)
            {
                handHeld.Ability(player.transform.position);
                lastAttackTime = Time.time;
            }

            // Show box if handHeld manager is active

            attackBox.SetActive(enabled);


            // Update direction based off player movement

>>>>>>> 2b74cbe (revamped weapons to handhelds for tools and added watering can):Sunspore/Assets/Scripts/Handhelds/HandHeldManager.cs
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
                
                Invoke(nameof(StopAttacking), handHeld.attackDuration);
            }
            Vector3 playerPos = player.transform.position;
            mouseWorldPos.y = 1.5f;
            Vector3 mouseDir = (mouseWorldPos - playerPos).normalized;
<<<<<<< HEAD:Sunspore/Assets/Scripts/Weapons/WeaponManager.cs
            weapon.Attack(boxCenter, hitArea, boxRotation, mouseDir, playerPos);
            playerScript.Swing();
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
=======
            handHeld.Attack(boxCenter, hitArea, boxRotation, mouseDir, playerPos);
        }

        private void OnDrawGizmos()
        {
            Quaternion boxRotation = currDirection != Vector3.zero
                ? Quaternion.LookRotation(currDirection)
                : player.transform.rotation;

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, boxRotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, hitArea);
        }

        // End of Attack
>>>>>>> 2b74cbe (revamped weapons to handhelds for tools and added watering can):Sunspore/Assets/Scripts/Handhelds/HandHeldManager.cs
        private void StopAttacking()
        {
            var renderer = attackBox.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = PreserveAlpha(renderer.material.color, Color.gray);
            }
            
        }
<<<<<<< HEAD:Sunspore/Assets/Scripts/Weapons/WeaponManager.cs
=======

        // Updates currDirection
>>>>>>> 2b74cbe (revamped weapons to handhelds for tools and added watering can):Sunspore/Assets/Scripts/Handhelds/HandHeldManager.cs
        public void setDirection(int dir)
        {
            currDirection = directions[dir-1];
        }
<<<<<<< HEAD:Sunspore/Assets/Scripts/Weapons/WeaponManager.cs
=======

        // Sets attackBox direction to currDirection
>>>>>>> 2b74cbe (revamped weapons to handhelds for tools and added watering can):Sunspore/Assets/Scripts/Handhelds/HandHeldManager.cs
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
<<<<<<< HEAD:Sunspore/Assets/Scripts/Weapons/WeaponManager.cs
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
            head = weapon.head;
            handle = weapon.handle;
            
            playerScript.weaponHead.TypeUpdate(head);
            playerScript.weaponBase.TypeUpdate(handle);
=======

        // Assign Stats 
        public void SetHandHeldStats(GameObject handHeldPrefab)
        {
            handHeld = handHeldPrefab.GetComponent<HandHeld>();
            offset = handHeld.attackRange / 2;
            hitArea.z = handHeld.attackRange;

>>>>>>> 2b74cbe (revamped weapons to handhelds for tools and added watering can):Sunspore/Assets/Scripts/Handhelds/HandHeldManager.cs
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