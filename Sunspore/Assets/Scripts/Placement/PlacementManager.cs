using System.Collections.Generic;
using System.Linq;
using Sunspore;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(InventoryManager))]
public class PlacementManager : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer, ignoreCastLayer;
    private const int NonClickableLayer = 1; // "Non Clickable"
    [SerializeField] Player player;

    Camera cam;
    InventoryManager inventoryManager;

    ItemSO currentItem;
    ItemSO draggedItem;
    InventorySlot draggedSlot;
    [SerializeField] GameObject preview;
    List<GameObject> currentPreviews = new();
    Vector3 draggedPreview;
    List<Vector2> footprints = new();
    readonly Dictionary<GameObject, LineRenderer> previewOutlines = new();

    const float epsilon = 0.01f;
    Color cannotPlace = new Color(245f / 255f, 103f / 255f, 105f / 255f);
    Color canPlace = new Color(103f / 255f, 245f / 255f, 105f / 255f);
    bool allowPlacement = false;

    private Coroutine placingCoroutine = null;
    private bool isPlacing = false;
    private float animTime = 0.75f;

    // SET TO SAME NUMBERS AS TERRAINTILEMAP SCALES
    private float zScale = 2.25f;
    private float xScale = 1.5f;


    void Awake()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

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

        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
        {
            PlaceDraggedItem();
        }

        if (player.anim.GetBool("isMoving") && placingCoroutine != null)
        {
            StopCoroutine(placingCoroutine);
            placingCoroutine = null;
            isPlacing = false;
        }

        if (inventoryManager.pointerDown && currentPreviews.Count > 0)
        {
            draggedPreview = currentPreviews[0].transform.position;
        }

        // Debug.Log("Allow Placement: " + allowPlacement);
    }

    void ShowPreview()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && ScreenToWorld(Input.mousePosition, out Vector3 position))
        {
            // Debug.Log("Placing Allowed");
            ItemSO item = null;
            if (inventoryManager.pointerDown)
            {
                item = inventoryManager.GetDraggedItemSO();
                draggedItem = item;
                draggedSlot = inventoryManager.GetDraggedItemSlot();
            }
            else
            {
                item = inventoryManager.GetSelectedItem();
            }
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
                }
                else
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
                    inst.transform.localScale = new Vector3(1.0f * xScale, 1.0f * zScale, 1.0f * zScale);
                    EnsureOutline(inst);
                    currentPreviews.Add(inst);
                }
            }
            foreach (var preview in currentPreviews)
            {
                float sizeX = preview.transform.localScale.x;
                float sizeZ = preview.transform.localScale.z;
                // Debug.Log(sizeX + " " + sizeZ);
                preview.transform.position = SnapPosition(position, sizeX, sizeZ);
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
        bool canPlaceAllPreviews = currentPreviews.Count > 0;

        foreach (var preview in currentPreviews)
        {
            bool isTilled = TerrainTilemap.Instance.CheckTilled(preview.transform.position);
            bool hasBlockingCollider = HasBlockingCollider(preview.transform.position, preview.transform.localScale);
            bool canPlaceHere = isTilled && !hasBlockingCollider;

            DrawSquareOutline(preview, canPlaceHere ? canPlace : cannotPlace);

            if (!canPlaceHere)
                canPlaceAllPreviews = false;
        }

        allowPlacement = canPlaceAllPreviews;
    }

    bool HasBlockingCollider(Vector3 previewPosition, Vector3 previewScale)
    {
        Vector3 halfExtents = new((previewScale.x - epsilon) / 2f, 0.5f, (previewScale.z - epsilon) / 2f);
        Collider[] colliders = Physics.OverlapBox(previewPosition, halfExtents, Quaternion.identity, ~(groundLayer | ignoreCastLayer));

        foreach (var collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }

            Plant plant = collider.GetComponentInParent<Plant>();
            if (plant != null)
            {
                // Ignore nearby plants unless they are on this exact planting tile.
                if (IsSameTile(plant.transform.position, previewPosition))
                {
                    return true;
                }

                continue;
            }

            return true;
        }

        return false;
    }

    bool IsSameTile(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) <= epsilon && Mathf.Abs(a.z - b.z) <= epsilon;
    }

    void EnsureOutline(GameObject previewObject)
    {
        if (previewObject == null || previewOutlines.ContainsKey(previewObject))
        {
            return;
        }

        var spriteRenderer = previewObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        GameObject outlineObject = new GameObject("PreviewOutline");
        outlineObject.transform.SetParent(previewObject.transform, false);
        outlineObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        LineRenderer line = outlineObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.widthMultiplier = 0.08f;
        line.loop = true;
        line.useWorldSpace = true;
        line.positionCount = 5;
        previewOutlines[previewObject] = line;
    }

    void DrawSquareOutline(GameObject previewObject, Color color)
    {
        if (previewObject == null)
        {
            return;
        }

        EnsureOutline(previewObject);
        if (!previewOutlines.TryGetValue(previewObject, out var line) || line == null)
        {
            return;
        }

        line.startColor = color;
        line.endColor = color;

        Vector3 center = previewObject.transform.position;
        Vector3 scale = previewObject.transform.localScale;
        float halfX = scale.x / 2f;
        float halfZ = scale.z / 2f;

        line.SetPosition(0, new Vector3(center.x - halfX, center.y + 0.01f, center.z - halfZ));
        line.SetPosition(1, new Vector3(center.x + halfX, center.y + 0.01f, center.z - halfZ));
        line.SetPosition(2, new Vector3(center.x + halfX, center.y + 0.01f, center.z + halfZ));
        line.SetPosition(3, new Vector3(center.x - halfX, center.y + 0.01f, center.z + halfZ));
        line.SetPosition(4, new Vector3(center.x - halfX, center.y + 0.01f, center.z - halfZ));
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
        if (allowPlacement && currentPreviews.Count > 0 && !isPlacing)
        {
            Vector3 currPlace = currentPreviews[0].transform.position;
            InventorySlot slot = inventoryManager.GetSelectedItemSlot();
            GameObject prefab = currentItem.prefab;
            placingCoroutine = StartCoroutine(PlaceAfterAnimation(currPlace, slot, prefab));
            isPlacing = true;
        }
    }

    void PlaceDraggedItem()
    {
        Debug.Log(allowPlacement + " " + isPlacing);
        if (allowPlacement && !isPlacing)
        {
            Vector3 currPlace = currentPreviews.Count > 0 ? currentPreviews[0].transform.position : draggedPreview;
            InventorySlot slot = draggedSlot;
            GameObject prefab = draggedItem.prefab;
            placingCoroutine = StartCoroutine(PlaceAfterAnimation(currPlace, slot, prefab));
            isPlacing = true;
        }
    }

    IEnumerator PlaceAfterAnimation(Vector3 currPlace, InventorySlot slot, GameObject prefab)
    {
        if (prefab.GetComponent<Plant>() == null || !TerrainTilemap.Instance.CheckTilled(currPlace))
        {
            placingCoroutine = null;
            isPlacing = false;
            yield return null;
        }
        player.Planting();

        yield return new WaitUntil(() => player.animFinished);
        player.anim.SetTrigger("Finished");
        SFXManager.Play("plant");
        //Debug.Log(player.animFinished);

        Instantiate(prefab, currPlace, Quaternion.identity);

        inventoryManager.UseThisItem(slot);
        isPlacing = false;
        TutorialManager.Instance.BroadcastSignal(TutorialManager.Actions.PLANT);
        placingCoroutine = null;
        //Debug.Log("done placing");
    }
    public bool ScreenToWorld(Vector2 screenPosition, out Vector3 worldPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);

        int mask = groundLayer & ~(1 << NonClickableLayer);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, mask))
        {
            worldPosition = hit.point;
            return true;
        }

        worldPosition = default;
        return false;
    }

    public Vector3 SnapPosition(Vector3 position, float sizeX, float sizeZ)
    {
        float snappedX = Mathf.Round(position.x / sizeX) * sizeX;
        float snappedZ = Mathf.Round(position.z / sizeZ) * sizeZ;
        //Debug.Log("xSnap" + snappedX + " zSnap " + snappedZ);
        return new Vector3(snappedX, position.y, snappedZ);
    }
}