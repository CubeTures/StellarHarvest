using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using static Codice.Client.BaseCommands.BranchExplorer.Layout.BrExLayout;
using UnityEngine.Tilemaps;
namespace Sunspore
{
    public class HandHeldManager : MonoBehaviour
    {
        public static HandHeldManager Instance { get; private set; }

        [SerializeField] GameObject player;

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
        private Player playerScript;

        private HandHeld handHeld;
        public HandHeld.WeaponHead head = HandHeld.WeaponHead.METAL;
        public HandHeld.WeaponHandle handle = HandHeld.WeaponHandle.WOOD;
        private Coroutine abilityCoroutine;

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

            hitArea = new Vector3(1.0f, 2.5f, 1.0f);
            directions[0] = new Vector3(0.0f, 0.0f, -1.0f);
            directions[1] = new Vector3(1.0f, 0.0f, 0.0f);
            directions[2] = new Vector3(0.0f, 0.0f, 1.0f);
            directions[3] = new Vector3(-1.0f, 0.0f, 0.0f);
            offset = 0;
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
                Attack(mouseWorldPos, false);
                lastAttackTime = Time.time;
            }

            // If right click cast ability

            if (enabled && handHeld.tool && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(1) && Time.time >= lastAttackTime + handHeld.attackCooldown)
            {
                if (handHeld.weapon)
                {
                    Vector3 mouseWorldPos = GetMouseWorldPosition();
                    Attack(mouseWorldPos, true);
                }
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (abilityCoroutine != null)
                    {
                        StopCoroutine(abilityCoroutine);
                    }
                    if(handHeld.CheckRange(player.GetComponent<Player>(), ray))
                    {
                        abilityCoroutine = StartCoroutine(AbilityDelayCoroutine(player.GetComponent<Player>(), ray, handHeld.abilityDuration));
                        lastAttackTime = Time.time;
                    }
                    else
                    {
                        OutOfRange();
                    }
                }
            }
            
            // Update direction based off player movement

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

        private void Attack(Vector3 mouseWorldPos, bool ability)
        {
            // Find all enemies in range
            updateDirection();
            Physics.SyncTransforms();
            Vector3 playerPos = player.transform.position;
            mouseWorldPos.y = 1.5f;
            Vector3 mouseDir = (mouseWorldPos - playerPos).normalized;
            if (ability)
            {
                handHeld.AttackWithAbility(boxCenter, hitArea, boxRotation, mouseDir, playerPos);
            }
            else
            {
                handHeld.Attack(boxCenter, hitArea, boxRotation, mouseDir, playerPos);
            }
            if (handHeld.ranged)
            {
                playerScript.Shoot(mouseDir);
            }
            else
            {
                playerScript.Swing();
            }
        }
        // private void OnDrawGizmos()
        // {
        //     Quaternion boxRotation = currDirection != Vector3.zero
        //         ? Quaternion.LookRotation(currDirection)
        //         : player.transform.rotation;
        //     Gizmos.color = Color.red;
        //     Gizmos.matrix = Matrix4x4.TRS(boxCenter, boxRotation, Vector3.one);
        //     Gizmos.DrawWireCube(Vector3.zero, hitArea * 1.5f);
        // }

        private IEnumerator AbilityDelayCoroutine(Player player, Ray ray, float delay)
        {
            yield return new WaitForSeconds(delay);
            handHeld.Ability(player, ray);
            abilityCoroutine = null;
        }

        public void CancelAbility()
        {
            if (abilityCoroutine == null)
            {
                return;
            }
            StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
            // Optionally, call a cancel method on the current handHeld if needed
            if (handHeld != null && handHeld.cancelable)
                handHeld.CancelAbility();
            Debug.Log("Ability cancelled!");
        }

        public void OutOfRange()
        {
            Debug.Log("out of range");
        }

        // Updates currDirection
        public void setDirection(int dir)
        {
            if (handHeld != null && handHeld.cancelable)
            {
                CancelAbility();
            }
            currDirection = directions[dir-1];
        }

        // Sets attackBox direction to currDirection
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

        // Assign Stats 
        public void SetHandHeldStats(GameObject handHeldPrefab)
        {
            handHeld = handHeldPrefab.GetComponent<HandHeld>();
            offset = handHeld.attackRange / 2;
            hitArea.z = handHeld.attackRange;
            playerScript.weaponHead.TypeUpdate(head);
            playerScript.weaponBase.TypeUpdate(handle);
            //hitArea.x = handHeld.abilityRange;

            //if (!melee)
            //{
            //    attackBox.SetActive(false);
            //}
            //else
            //{
            //    attackBox.SetActive(true);
            //}
        }
    }
}