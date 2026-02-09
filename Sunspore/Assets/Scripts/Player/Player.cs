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
    public WeaponManager weaponManager;

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

    float _vx;
    float _vy;
    float _vz;

    // Camera rotation tracking
    float _rotationX = 0f; // Horizontal rotation (Y-axis)
    float _rotationY = 0f; // Vertical rotation (X-axis)

    bool _facingRight = true;
    bool _facingFront = true;

    void Start()
    {
        _transform = GetComponent<Transform>();
        _anim = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        _mainCamera = Camera.main;
        weaponManager = GameObject.Find("Weapon Manager").GetComponent<WeaponManager>();

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
    }

    public void SetMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        _vx = input.x;
        _vz = input.y;

        // Swap sprites based on the direction the player is moving
        if (_vx > 0 && _vx > _vz)
        {
            _facingRight = true;
            _anim.SetInteger("Direction", 3);
            weaponManager.setDirection(1);
        }
        else if (_vx < 0)
        {
            _facingRight = false;
            _anim.SetInteger("Direction", 2);
            weaponManager.setDirection(3);
        }

        if (_vz > 0 && _vz > _vx)
        {
            _facingFront = true;
            _anim.SetInteger("Direction", 1);
            weaponManager.setDirection(0);
        }
        else if (_vz < 0)
        {
            _facingFront = false;
            _anim.SetInteger("Direction", 0);
            weaponManager.setDirection(2);
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
}
