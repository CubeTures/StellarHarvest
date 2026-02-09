using System.Collections.Generic;
using System.Linq;
using Sunspore;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InventoryManager))]
public class PlacementManager : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;

    Camera cam;
    InventoryManager inventoryManager;

    ItemSO currentItem;
    [SerializeField] GameObject preview;
    List<GameObject> currentPreviews = new();
    List<Vector2> footprints = new();

    const float epsilon = 0.01f;
    Color cannotPlace = new Color(245f / 255f, 103f / 255f, 105f / 255f);
    Color canPlace = new Color(103f / 255f, 245f / 255f, 105f / 255f);
    bool allowPlacement = false;


    void Awake()
    {
        cam = Camera.main;
        inventoryManager = GetComponent<InventoryManager>();
    }

    void Update()
    {
        ShowPreview();
        ColorPreviews();

        // placement
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            PlaceItem();
        }
    }

    void ShowPreview()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && ScreenToWorld(Input.mousePosition, out Vector3 position))
        {
            ItemSO item = inventoryManager.GetSelectedItem();
            if (item == null || !item.Equals(currentItem))
            {
                foreach (var _ in footprints)
                {
                    Destroy(currentPreviews[0]);
                    currentPreviews.RemoveAt(0);
                }

                if (item != null)
                {
                    currentItem = item;
                    GetFootprints(item);
                } else
                {
                    currentItem = null;
                    footprints.Clear();
                }
            }

            if (currentPreviews.Count == 0)
            {
                foreach (var footprint in footprints)
                {
                    GameObject inst = Instantiate(preview, position, Quaternion.Euler(90f, 0, 0));
                    inst.transform.localScale = new Vector3(footprint.x, footprint.y, inst.transform.localScale.z);
                    currentPreviews.Add(inst);
                }
            }
            foreach (var preview in currentPreviews)
            {
                preview.transform.position = SnapPosition(position);
                preview.SetActive(true);
            }
        }
        else
        {
            foreach (var preview in currentPreviews)
            {
                preview.SetActive(false);
            }
            allowPlacement = false;
        }
    }

    void ColorPreviews()
    {
        foreach (var preview in currentPreviews)
        {
            var sr = preview.GetComponent<SpriteRenderer>();

            Vector3 box = new((preview.transform.localScale.x - epsilon) / 2, 0.5f, (preview.transform.localScale.z - epsilon) / 2);
            var colliders = Physics.OverlapBox(preview.transform.position, box, Quaternion.identity, ~groundLayer);

            if (colliders.Count() > 0)
            {
                sr.color = cannotPlace;
                allowPlacement = false;
            }
            else
            {
                sr.color = canPlace;
                allowPlacement = true;
            }
        }
    }

    void GetFootprints(ItemSO item)
    {
        footprints.Clear();

        if (item.placeable)
        {
            GameObject prefab = item.prefab;
            if (prefab.GetComponent<Plant>() is Plant plant)
            {
                // TODO iterate over all plant prefabs, record their footprints
                footprints.Add(GetPrefabSize(prefab));
            }
            else
            {
                footprints.Add(GetPrefabSize(prefab));
            }
        }
    }

    Vector2 GetPrefabSize(GameObject prefab)
    {
        return new Vector2(prefab.transform.localScale.x, prefab.transform.localScale.z);
    }

    void PlaceItem()
    {
        if (allowPlacement && currentPreviews.Count > 0)
        {
            GameObject prefab = currentItem.prefab;
            Instantiate(prefab, currentPreviews[0].transform.position, Quaternion.identity);
            inventoryManager.UseSelectedItem();
        }
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