using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
namespace Sunspore
{
    public class HandHeldManager : MonoBehaviour
    {
        public static HandHeldManager Instance { get; private set; }
        private const int NonClickableLayer = 1; // "Non Clickable"

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
        private Player playerScript;

        private HandHeld handHeld;
        public HandHeld.WeaponHead head = HandHeld.WeaponHead.METAL;
        public HandHeld.WeaponHandle handle = HandHeld.WeaponHandle.WOOD;
        private Coroutine abilityCoroutine;
        [SerializeField] LineRenderer abilityRangeRing;
        [SerializeField] private int ringSegments = 64;

        private float castTime = 0;
        public UnityEvent<float> OnAbility;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                player = GameObject.FindGameObjectWithTag("Player");
                playerScript = player.GetComponent<Player>();
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
            
            // Create the LineRenderer for the ability range ring
            abilityRangeRing = GetComponent<LineRenderer>();
            abilityRangeRing.transform.SetParent(transform);
            abilityRangeRing.widthMultiplier = 0.05f;
            abilityRangeRing.loop = true;
            abilityRangeRing.positionCount = ringSegments + 1;
            abilityRangeRing.enabled = false;
            abilityRangeRing.useWorldSpace = true;

            OnAbility?.Invoke(0);
        }
        private void Update()
        {
            // If lieft click attack

            if (enabled && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + handHeld.attackCooldown && !TutorialManager.Instance.IsTutorialActive)
            {
                Vector3 mouseWorldPos = GetMouseWorldPosition();
                Attack(mouseWorldPos, false);
                lastAttackTime = Time.time;
            }

            // If right click cast ability

            if (enabled && handHeld.tool && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(1) && Time.time >= lastAttackTime + handHeld.attackCooldown && (!player.GetComponent<Player>().anim.GetBool("isMoving") || handHeld.weapon))
            {
                if (handHeld.weapon)
                {
                    Vector3 mouseWorldPos = GetMouseWorldPosition();
                     Attack(mouseWorldPos, true);
                }
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if(abilityCoroutine == null && handHeld.CheckRange(player.GetComponent<Player>(), ray))
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

            if (enabled && handHeld != null && handHeld.tool && !handHeld.weapon)
            {
                // Show the ring at the player's position with the ability range
                DrawAbilityRangeRing(player.transform.position, handHeld.abilityRange);
            }
            else
            {
                abilityRangeRing.enabled = false;
            }

            // Show ability cooldown ring if ability is active
            if(abilityCoroutine != null)
            {
                OnAbility?.Invoke((Time.time - castTime) / handHeld.abilityDuration);
            }
        }
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
            int mask = LayerMask.GetMask("Ground") & ~(1 << NonClickableLayer);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
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
            player.anim.SetTrigger("Plant");
            castTime = Time.time;
            yield return new WaitForSeconds(delay);
            Debug.Log("ability cast");
            handHeld.Ability(player, ray);
            abilityCoroutine = null;
            OnAbility?.Invoke(0);
            player.anim.SetInteger("Direction", player.currentDirection);
            player.anim.SetTrigger("Finished");
        }

        public void CancelAbility()
        {
            if (abilityCoroutine == null)
            {
                return;
            }
            StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
            OnAbility?.Invoke(0);
            // Optionally, call a cancel method on the current handHeld if needed
            if (handHeld != null && handHeld.cancelable)
                handHeld.CancelAbility();
            Debug.Log("Ability cancelled!");
        }

        public void OutOfRange()
        {
            Debug.Log("out of range");
        }

        private void DrawAbilityRangeRing(Vector3 center, float radius)
        {
            if (abilityRangeRing == null) return;
            abilityRangeRing.enabled = true;
            float zScale = 1.5f; // Adjust this factor as needed for your camera

            for (int i = 0; i <= ringSegments; i++)
            {
                float angle = 2 * Mathf.PI * i / ringSegments;
                float x = center.x + radius * Mathf.Cos(angle);
                float z = center.z + radius * Mathf.Sin(angle) * zScale;
                abilityRangeRing.SetPosition(i, new Vector3(x, 0 + 0.05f, z));
            }
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