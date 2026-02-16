using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Diagnostics.Contracts;
using Sunspore;

public class Player : MonoBehaviour
{
    public float speed = 8f;
    public GameObject[] inventory;
    public float gridSize = 1f; // Size of each grid cell
    [SerializeField] float health = 100;
    [SerializeField] private SpriteRenderer _sr;

    [HideInInspector]
    public RaycastHit selectedObject;
    private Transform hoveredObject;

    [HideInInspector]
    public bool playerCanMove = true;
    [HideInInspector]
    public bool isSelecting = false;

    Transform _transform;
    Animator _anim;
    CapsuleCollider _collider;
    Rigidbody _rb;
    AudioSource _audioSource;
    Camera _mainCamera;
    CinemachineCamera _cinemachineCamera;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    float _vx;
    float _vy;
    float _vz;

    // Camera rotation tracking
    float _rotationX = 0f; // Horizontal rotation (Y-axis)
    float _rotationY = 0f; // Vertical rotation (X-axis)

    bool _facingRight = true;
    bool _facingFront = true;
    int _currentDirection = -1;

    void Start()
    {
        _transform = GetComponent<Transform>();
        _anim = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        _sr = GetComponent<SpriteRenderer>();
        _mainCamera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if(_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
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
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.red;
        }
    }

    public void SetMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        _vx = input.x;
        _vz = input.y;

        bool isMoving = input.sqrMagnitude > 0.01f;
        _anim.SetBool("isMoving", isMoving);

        if (!isMoving)
            return;

        // Decide direction based on dominant axis
        if (Mathf.Abs(_vx) > Mathf.Abs(_vz))
        {
            // Horizontal wins
            if (_vx > 0)
            {
                _facingRight = true;
                SetDirection(1); // Right
                WeaponManager.Instance.setDirection(1);
            }
            else
            {
                _facingRight = false;
                SetDirection(2); // Left
                WeaponManager.Instance.setDirection(2);
            }
        }
        else
        {
            // Vertical wins
            if (_vz > 0)
            {
                _facingFront = true;
                SetDirection(4); // Forward
                WeaponManager.Instance.setDirection(4);
            }
            else
            {
                _facingFront = false;
                SetDirection(3); // Back
                WeaponManager.Instance.setDirection(3);
            }
        }
    if (_facingRight)
        {
            _sr.flipX = true;   
        }
    else
        {
            _sr.flipX = false;
        }

    }



    public void SetDirection(int dir)
    {
        _currentDirection = dir;
        _anim.SetInteger("Direction", dir);
        _anim.SetBool("isMoving", true);
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
        health -= damage;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.red;
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), 0.2f); // 0.2 seconds, adjust as needed
        }
        if (health <= 0)
        {
            //Destroy(this.gameObject);
        }
    }

    private void ResetColor()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
    }
}
