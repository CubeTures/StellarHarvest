using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 8f;
    public GameObject[] inventory;
    public float gridSize = 1f; // Size of each grid cell

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
        }
        else if (_vx < 0)
        {
            _facingRight = false;
            _anim.SetInteger("Direction", 2);
        }

        if (_vz > 0 && _vz > _vx)
        {
            _facingFront = true;
            _anim.SetInteger("Direction", 1);
        }
        else if (_vz < 0)
        {
            _facingFront = false;
            _anim.SetInteger("Direction", 0);
        }
    }

    public void Move()
    {
        Vector3 moveDirection = _transform.TransformDirection(new Vector3(_vx, 0, _vz));
        Vector3 targetVelocity = moveDirection * speed;
        Vector3 velocityDiff = targetVelocity - new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        _rb.AddForce(velocityDiff * 15f, ForceMode.Acceleration);
    }

    public void Select(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            // Player started holding the button
            isSelecting = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            // Player released the button
            isSelecting = false;
        }

        if (context.phase == InputActionPhase.Performed)
        {
            // Ensure camera is initialized
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    return;
                }
            }

            RaycastHit hit;
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                selectedObject = hit;
                // Draw green line to show successful raycast hit
                Debug.DrawLine(ray.origin, hit.point, Color.green, 2f);

                // If clicked on layer ground, instantiate a plant at grid-aligned position
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    Vector3 snappedPosition = SnapToGrid(hit.point);
                    Instantiate(inventory[0], snappedPosition, Quaternion.identity);
                }
            }
            else
            {
                selectedObject = new RaycastHit();
                // Draw red line to show raycast miss (draw for 100 units)
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
            }
        }
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

    // Snap position to grid
    Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = position.y; // Keep original Y position (ground height)
        float z = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(x, y, z);
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
