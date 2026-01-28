using UnityEngine;

[RequireComponent(typeof(InventoryManager))]
public class PlacementManager : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;

    Camera cam;
    InventoryManager inventoryManager;

    InventoryItem currentItem;
    [SerializeField] GameObject preview;
    GameObject currentPreview;

    void Awake()
    {
        cam = Camera.main;
        inventoryManager = GetComponent<InventoryManager>();
    }

    void Update()
    {
        ShowPreview();

        // placement
        if (Input.GetKeyDown(KeyCode.E))
        {
            PlaceItem();
        }
    }

    void ShowPreview()
    {
        // get item
        // get footprint of prefab
        // show highlight
        if (ScreenToWorld(Input.mousePosition, out Vector3 position))
        {
            if (inventoryManager.GetSelectedItem() is InventoryItem item && !item.Equals(currentItem))
            {
                currentItem = item;
            }

            if (currentPreview == null)
            {
                currentPreview = Instantiate(preview, position, Quaternion.identity);
            }
            currentPreview.transform.position = SnapPosition(position);
            currentPreview.SetActive(true);
        }
        else
        {
            if (currentPreview != null)
            {
                currentPreview.SetActive(false);
            }
        }
    }

    void PlaceItem()
    {
        // check overlap
        // get item
        // decrement amount
        // place in world space
    }

    public bool ScreenToWorld(Vector2 screenPosition, out Vector3 worldPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            worldPosition = hit.point;
            return true;
        }

        worldPosition = default;
        return false;
    }

    public Vector3 SnapPosition(Vector3 position)
    {
        return new Vector3
        {
            x = Mathf.RoundToInt(position.x),
            y = position.y + 0.01f,
            z = Mathf.RoundToInt(position.z),
        };
    }
}