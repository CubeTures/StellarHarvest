using UnityEngine;
using TMPro;
using Sunspore;

[RequireComponent(typeof(Collider))]
public class StoreWorldInteractable : MonoBehaviour
{
    private KeyCode interactKey = KeyCode.Space;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private bool autoUpdatePromptText = false;
    [SerializeField] private TMP_Text interactPromptLabel;
    [SerializeField] private string interactPromptText = "Press I to interact";
    [SerializeField] private string closePromptText = "Press I to close";
    [SerializeField] private bool closeStoreWhenPlayerLeaves = true;
    [Header("Fallback Detection")]
    [SerializeField] private bool useDistanceFallback = true;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private GameObject hologram;

    private Animator holoAnim;
    private bool _playerInRange;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        if (hologram != null)
        {
            holoAnim = hologram.GetComponent<Animator>();

            var srThis = GetComponent<SpriteRenderer>();
            var srHolo = hologram.GetComponent<SpriteRenderer>();
            if (srThis != null && srHolo != null)
            {
                srThis.sortingOrder = srHolo.sortingOrder;
            }
        }

        if (autoUpdatePromptText && interactPromptLabel == null && interactPrompt != null)
        {
            interactPromptLabel = interactPrompt.GetComponentInChildren<TMP_Text>(true);
        }

        UpdatePromptText(false);

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }


    }

    private void Update()
    {
        if (useDistanceFallback && playerTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, transform.position);
            _playerInRange = distance <= interactionRadius;
            if (interactPrompt != null)
            {
                bool shouldShowPrompt = _playerInRange && (NewStoreManager.Instance == null || !NewStoreManager.Instance.IsOpen);
                interactPrompt.SetActive(shouldShowPrompt);
            }

            if (_playerInRange)
            {
                UpdatePromptText(NewStoreManager.Instance != null && NewStoreManager.Instance.IsOpen);
            }
        }

        if (!_playerInRange || NewStoreManager.Instance == null)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            if (TutorialManager.Instance.IsTutorialActive && !TutorialManager.Instance.availableActions[TutorialManager.Actions.INVENTORY]) return;

            InventoryManager.Instance.CloseInventory();
            bool isStoreOpen = NewStoreManager.Instance.Toggle();
            UpdatePromptText(isStoreOpen);
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(!isStoreOpen);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        _playerInRange = true;
        if (interactPrompt != null)
        {
            UpdatePromptText(false);
            interactPrompt.SetActive(true);
            holoAnim.SetTrigger("Enter");
            SFXManager.Play("hologramactivate");
            SFXManager.PlayLoop("hologramhum");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        _playerInRange = false;
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
            holoAnim.SetTrigger("Exit");
            SFXManager.StopLoop("hologramhum");
            SFXManager.Play("hologramdeactivate");
        }

        if (closeStoreWhenPlayerLeaves && NewStoreManager.Instance != null && NewStoreManager.Instance.IsOpen)
        {
            NewStoreManager.Instance.Close();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!useDistanceFallback)
        {
            return;
        }

        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawSphere(transform.position, interactionRadius);
    }

    private void UpdatePromptText(bool storeOpen)
    {
        if (!autoUpdatePromptText || interactPromptLabel == null)
        {
            return;
        }

        interactPromptLabel.text = storeOpen ? closePromptText : interactPromptText;
    }
}
