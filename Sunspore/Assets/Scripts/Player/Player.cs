using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Diagnostics.Contracts;
using Sunspore;

public class Player : MonoBehaviour
{
    [SerializeField] Wallet wallet;
    public WeaponHead weaponHead;
    public WeaponHandle weaponBase;
    public float speed = 8f;
    public GameObject[] inventory;
    public float gridSize = 1f; // Size of each grid cell
    [SerializeField] float health = 100;

    [HideInInspector]
    public RaycastHit selectedObject;
    private Transform hoveredObject;

    [HideInInspector]
    public bool playerCanMove = true;
    [HideInInspector]
    public bool isSelecting = false;
    [HideInInspector]
    public bool isDead = false;
    public bool animFinished = false;
    Transform _transform;
    public Animator anim;
    CapsuleCollider _collider;
    Rigidbody _rb;
    AudioSource _audioSource;
    Camera _mainCamera;
    CinemachineCamera _cinemachineCamera;
    public SpriteRenderer spriteRenderer;
    private Color _originalColor;

    GameManager gm;

    float _vx;
    float _vy;
    float _vz;

    // Camera rotation tracking
    float _rotationX = 0f; // Horizontal rotation (Y-axis)
    float _rotationY = 0f; // Vertical rotation (X-axis)

    bool _facingRight = true;
    bool _facingFront = true;
    public int currentDirection = -1;

    void Start()
    {
        _transform = GetComponent<Transform>();
        anim = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        _mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        gm = GameManager.gm;
        if(spriteRenderer != null)
        {
            _originalColor = spriteRenderer.color;
        }

        // Find and setup Cinemachine Virtual Camera
        _cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

        // Ensure EventSystem exists
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        // Delay cursor lock to next frame to ensure input system is initialized
        StartCoroutine(InitializeCursorState());
    }

    IEnumerator InitializeCursorState()
    {
        // Wait for end of frame to ensure everything is initialized
        yield return new WaitForEndOfFrame();

        if (PlayerPrefManager.GetMouseWindowLock())
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
    void FixedUpdate()
    {
        // Exit update if game is paused
        if (Time.timeScale == 0f)
            return;

        Move();

        // If player fell out of the map, respawn
        if (transform.position.y < -10f)
        {
            Respawn(GameManager.gm.transform.position + new Vector3(0, 1.5f, 0));
        }

        if (health == 10)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.red;
        }

        spriteRenderer.sortingOrder = (int)(-transform.position.z * 100);

    }

    public void SetMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        _vx = input.x;
        _vz = input.y;

        bool isMoving = input.sqrMagnitude > 0.01f;
        anim.SetBool("isMoving", isMoving);
        anim.SetInteger("Direction", currentDirection);
        
        anim.ResetTrigger("Plant");

        if (!isMoving || isDead)
            return;

        // Decide direction based on dominant axis
        if (Mathf.Abs(_vx) > Mathf.Abs(_vz))
        {
            // Horizontal wins
            if (_vx > 0)
            {
                _facingRight = true;
                SetDirection(2); // Right
            }
            else
            {
                _facingRight = false;
                SetDirection(4); // Left
            }
        }
        else
        {
            // Vertical wins
            if (_vz > 0)
            {
                _facingFront = true;
                SetDirection(3); // Forward
            }
            else
            {
                _facingFront = false;
                SetDirection(1); // Back            
            }
        }

    }

    public void SetDirection(int dir)
    {
        currentDirection = dir;
        HandHeldManager.Instance.setDirection(dir);
        weaponHead.SetSwingTransform(dir, spriteRenderer.flipX);
        weaponBase.SetWeaponBaseTransform(dir, spriteRenderer.flipX);
        anim.SetInteger("Direction", dir);
        anim.SetBool("isMoving", true);
        
        if (_vx > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (_vx < 0)
        {
            spriteRenderer.flipX = false;
        }
    }
    public void Move()
    {
        Vector3 moveDirection = _transform.TransformDirection(new Vector3(_vx, 0, _vz));
        Vector3 targetVelocity = moveDirection * speed;
        Vector3 velocityDiff = targetVelocity - new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        _rb.AddForce(velocityDiff * 15f, ForceMode.Acceleration);
    }

    // public function to respawn the player at the appropriate location
    public void Respawn(Vector3 spawnloc)
    {
        UnFreezeMotion();
        _transform.parent = null;
        _transform.position = spawnloc;
    }

    // do what needs to be done to freeze the player
    public void FreezeMotion()
    {
        playerCanMove = false;
        if (!_rb.isKinematic)
        {
            _rb.linearVelocity = Vector3.zero;
        }
        _rb.isKinematic = true;
    }

    // do what needs to be done to unfreeze the player
    void UnFreezeMotion()
    {
        playerCanMove = true;
        _rb.isKinematic = false;
        Debug.Log("Player unfrozen");
    }

    public void PlaySound(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }

    // Method to handle cursor locked/unlocked when game is paused
    public void SetPauseState(bool isPaused)
    {
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (PlayerPrefManager.GetMouseWindowLock())
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
        Cursor.visible = true;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        health -= damage;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), 0.2f); // 0.2 seconds, adjust as needed
        }
        if (health <= 0)
        {
            StartCoroutine(RespawnWithDelay(5f));
        }
    }

    private IEnumerator RespawnWithDelay(float delay)
    {
        FreezeMotion();
        isDead = true;
        yield return new WaitForSeconds(delay);
        isDead = false;
        health = 100;
        wallet.Subtract(wallet.GetAmount() / 2);
        UnFreezeMotion();
        Respawn(GameManager.gm.transform.position + new Vector3(0, 1.5f, 0));
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = _originalColor;
    }
    public void Planting()
    {
        anim.SetTrigger("Plant");
        StartCoroutine(AnimationFinished());
    }

    public void Swing()
    {
        anim.SetTrigger("Swing");
        weaponHead.Swing();
        weaponBase.Swing();
        StartCoroutine(AnimationFinished());
    }

    private IEnumerator AnimationFinished()
    {
        animFinished = false;
        yield return new WaitUntil(() => animFinished);
        anim.SetInteger("Direction", currentDirection);
        anim.SetTrigger("Finished");
        //Debug.Log("finished animation");
    }
}