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
        public HandHeld.WeaponHead head;
        public HandHeld.WeaponHandle handle = HandHeld.WeaponHandle.WOOD;
        private Coroutine abilityCoroutine;
        [SerializeField] LineRenderer abilityRangeRing;
        [SerializeField] LineRenderer targetPreviewRing;
        [SerializeField] private int ringSegments = 64;

        private float castTime = 0;
        public UnityEvent<float> OnAbility;
        private HandHeld lastUsedHandHeld; // Track which handHeld was last used

        private float zScale = 1.0f;
        private float xScale = 1.0f;
        private float zOffset = 1.5f;
        private bool z = false;

        private HandHeld draggedHandHeld;

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

            hitArea = new Vector3(1.25f, 2.5f, 1.0f);
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

            if (targetPreviewRing == null)
            {
                GameObject ringObject = new GameObject("TargetPreviewRing");
                ringObject.transform.SetParent(transform);
                targetPreviewRing = ringObject.AddComponent<LineRenderer>();
                targetPreviewRing.material = new Material(Shader.Find("Sprites/Default"));
            }

            targetPreviewRing.widthMultiplier = 0.08f;
            targetPreviewRing.loop = true;
            targetPreviewRing.positionCount = ringSegments + 1;
            targetPreviewRing.enabled = false;
            targetPreviewRing.useWorldSpace = true;

            OnAbility?.Invoke(0);
        }
        private void Update()
        {
            // If left click attack
            if (enabled && handHeld != null && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + handHeld.attackCooldown && !TutorialManager.Instance.IsTutorialActive)
            {
                Vector3 mouseWorldPos = GetMouseWorldPosition();
                Attack(mouseWorldPos, false);
                lastAttackTime = Time.time;
                lastUsedHandHeld = handHeld;
            }

            // If right click cast ability
            Debug.Log(EventSystem.current.IsPointerOverGameObject() + " " + EventSystem.current.gameObject.name);
            if (enabled && handHeld!=null && handHeld.tool && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(1) && Time.time >= lastAttackTime + handHeld.attackCooldown && (!player.GetComponent<Player>().anim.GetBool("isMoving") || handHeld.weapon))
            {
                lastUsedHandHeld = handHeld;
                if (handHeld.weapon)
                {
                    Vector3 mouseWorldPos = GetMouseWorldPosition();
                    Attack(mouseWorldPos, true);
                }
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    bool canCast = abilityCoroutine == null && handHeld.CheckRange(player.GetComponent<Player>(), ray);
                    Debug.Log($"HandHeld: {handHeld.name}, CheckRange: {canCast}, AbilityCooldown Ready: {Time.time >= lastAttackTime + handHeld.attackCooldown}");

                    if (abilityCoroutine == null && handHeld.CheckRange(player.GetComponent<Player>(), ray))
                    {
                        abilityCoroutine = StartCoroutine(AbilityDelayCoroutine(player.GetComponent<Player>(), ray, handHeld.abilityDuration, handHeld));
                        lastAttackTime = Time.time;
                    }
                    else
                    {
                        OutOfRange();
                    }
                }
            }

            // If click and drag tool cast ability

            if (draggedHandHeld != null && draggedHandHeld.tool && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0) && Time.time >= lastAttackTime + draggedHandHeld.attackCooldown && (!player.GetComponent<Player>().anim.GetBool("isMoving") || draggedHandHeld.weapon))
            {
                if (!draggedHandHeld.weapon)
                {
                    lastUsedHandHeld = draggedHandHeld;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (abilityCoroutine == null && draggedHandHeld.CheckRange(player.GetComponent<Player>(), ray))
                    {
                        abilityCoroutine = StartCoroutine(AbilityDelayCoroutine(player.GetComponent<Player>(), ray, draggedHandHeld.abilityDuration, draggedHandHeld));
                        lastAttackTime = Time.time;
                        draggedHandHeld = null;
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

            if ((enabled && handHeld != null && (handHeld.tool || handHeld is Scythe)) || draggedHandHeld != null)
            {
                HandHeld handHeldPreview = draggedHandHeld != null ? draggedHandHeld : handHeld;
                // Show the ring at the player's position with the ability range
                DrawAbilityRangeRing(player.transform.position, handHeldPreview.abilityRange);

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (handHeldPreview.TryGetAbilityPreview(playerScript, ray, out Vector3 previewCenter, out Vector3 previewSize, out bool canUse))
                {
                    DrawTargetPreviewRing(previewCenter, previewSize, canUse ? Color.green : Color.red);
                }
                else if (targetPreviewRing != null)
                {
                    targetPreviewRing.enabled = false;
                }
            }
            else
            {
                abilityRangeRing.enabled = false;
                if (targetPreviewRing != null)
                {
                    targetPreviewRing.enabled = false;
                }
            }

            // Show ability cooldown ring if ability is active
            if (abilityCoroutine != null && lastUsedHandHeld != null)
            {
                OnAbility?.Invoke((Time.time - castTime) / lastUsedHandHeld.abilityDuration);
            }

            // Get dragged item (seeds, etc. have no HandHeld on prefab — must null-check the chain)
            if (InventoryManager.Instance != null && InventoryManager.Instance.pointerDown)
            {
                var itemSo = InventoryManager.Instance.GetDraggedItemSO();
                if (itemSo != null && itemSo.prefab != null)
                {
                    draggedHandHeld = itemSo.prefab.GetComponent<HandHeld>();
                }
                else
                {
                    draggedHandHeld = null;
                }
            }
            else
            {
                draggedHandHeld = null;
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
                handHeld.AttackWithAbility(boxCenter, hitArea, boxRotation, mouseDir, playerPos, z);
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
        private void OnDrawGizmos()
        {
            if (player == null)
            {
                return;
            }
            Quaternion boxRotation = currDirection != Vector3.zero
                ? Quaternion.LookRotation(currDirection)
                : player.transform.rotation;
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, boxRotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, hitArea * 1.5f);
        }

        private IEnumerator AbilityDelayCoroutine(Player player, Ray ray, float delay, HandHeld abilityHandHeld)
        {
            player.anim.SetTrigger("Plant");
            castTime = Time.time;
            // If the current handHeld declares a looping SFX key, start looping now
            string loopKey = abilityHandHeld != null ? abilityHandHeld.AbilityLoopSfxKey() : null;
            bool loopStarted = false;
            if (!string.IsNullOrEmpty(loopKey))
            {
                SFXManager.PlayLoop(loopKey);
                loopStarted = true;
            }

            yield return new WaitForSeconds(delay);
            //Debug.Log("ability cast");
            abilityHandHeld.Ability(player, ray);
            // stop loop if we started one
            if (loopStarted && !string.IsNullOrEmpty(loopKey))
            {
                SFXManager.StopLoop(loopKey);
            }
            OnAbility?.Invoke(0);
            player.anim.SetInteger("Direction", player.currentDirection);
            player.anim.SetTrigger("Finished");
            Debug.Log("Player animFinished: " + player.animFinished);
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
            OnAbility?.Invoke(0);

            // Stop any looping SFX that was started by AbilityDelayCoroutine
            if (lastUsedHandHeld != null)
            {
                string loopKey = lastUsedHandHeld.AbilityLoopSfxKey();
                if (!string.IsNullOrEmpty(loopKey))
                    SFXManager.StopLoop(loopKey);
            }

            if (lastUsedHandHeld != null && lastUsedHandHeld.cancelable)
                lastUsedHandHeld.CancelAbility();
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

        private void DrawTargetPreviewRing(Vector3 center, Vector3 size, Color color)
        {
            if (targetPreviewRing == null) return;

            targetPreviewRing.enabled = true;
            targetPreviewRing.startColor = color;
            targetPreviewRing.endColor = color;

            float halfWidth = Mathf.Max(0.4f, size.x / 2f);
            float halfDepth = Mathf.Max(0.4f, size.z / 2f);

            targetPreviewRing.positionCount = 5;
            targetPreviewRing.SetPosition(0, new Vector3(center.x - halfWidth, center.y + 0.05f, center.z - halfDepth));
            targetPreviewRing.SetPosition(1, new Vector3(center.x + halfWidth, center.y + 0.05f, center.z - halfDepth));
            targetPreviewRing.SetPosition(2, new Vector3(center.x + halfWidth, center.y + 0.05f, center.z + halfDepth));
            targetPreviewRing.SetPosition(3, new Vector3(center.x - halfWidth, center.y + 0.05f, center.z + halfDepth));
            targetPreviewRing.SetPosition(4, new Vector3(center.x - halfWidth, center.y + 0.05f, center.z - halfDepth));
        }

        // Updates currDirection
        public void setDirection(int dir)
        {
            if (lastUsedHandHeld != null && lastUsedHandHeld.cancelable)
            {
                CancelAbility();
            }
            currDirection = directions[dir - 1];
            Debug.Log("Direction set to: " + currDirection);
        }

        // Sets attackBox direction to currDirection
        private void updateDirection()
        {
            hitArea.z = 1.0f * handHeld.attackRange;
            hitArea.x = 1.25f;

            // Update scale of attackBox based on direction
            if (currDirection == Vector3.forward || currDirection == Vector3.back)
            {
                zScale = 1.5f;
                xScale = 1.0f;
                z = true;
            }
            else if (currDirection == Vector3.right || currDirection == Vector3.left)
            {
                zScale = 1.0f;
                xScale = 1.5f;
                z = false;
            }
            hitArea.z *= zScale;
            hitArea.x *= xScale;

            offset = hitArea.z / 2;

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
            playerScript.weaponHead.TypeUpdate(handHeld.head, handHeld);
            playerScript.weaponBase.TypeUpdate(handle);
            playerScript.weaponBase.SetHeadSprite(handHeld.headSprite);
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