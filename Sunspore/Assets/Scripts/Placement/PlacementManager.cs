using System.Collections.Generic;
using System.Linq;
using Sunspore;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(InventoryManager))]
public class PlacementManager : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer, ignoreCastLayer;
    private const int NonClickableLayer = 1; // "Non Clickable"
    [SerializeField]  Player player;

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

    private Coroutine placingCoroutine = null;
    private bool isPlacing = false;
    private float animTime = 0.75f;

    // SET TO SAME NUMBERS AS TERRAINTILEMAP SCALES
    private float zScale = 2.25f;
    private float xScale = 1.5f;


    void Awake()
    {
        if(player == null)
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

        if (player.anim.GetBool("isMoving") && placingCoroutine != null)
        {
            StopCoroutine(placingCoroutine);
            placingCoroutine = null;
            isPlacing = false;
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
                    inst.transform.localScale = new Vector3(1.0f * xScale, 1.0f * zScale, 1.0f * zScale);
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
            // Debug.Log("FALSE");
            allowPlacement = false;
        }
    }

    void ColorPreviews()
    {
        foreach (var preview in currentPreviews)
        {
            var sr = preview.GetComponent<SpriteRenderer>();

            Vector3 box = new((preview.transform.localScale.x - epsilon) / 2, 0.5f, (preview.transform.localScale.z - epsilon) / 2);
            var colliders = Physics.OverlapBox(preview.transform.position, box, Quaternion.identity, ~(groundLayer | ignoreCastLayer));
            
            // updated the check to 1 from 0 because of camera bounds game object
            if (colliders.Count() > 1 || !TerrainTilemap.Instance.CheckTilled(preview.transform.position))
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
        if (allowPlacement && currentPreviews.Count > 0 && !isPlacing)
        {
            Vector3 currPlace = currentPreviews[0].transform.position;
            InventorySlot slot = inventoryManager.GetSelectedItemSlot();
            placingCoroutine = StartCoroutine(PlaceAfterAnimation(currPlace, slot));
            isPlacing = true;
        }
    }

    IEnumerator PlaceAfterAnimation(Vector3 currPlace, InventorySlot slot)
    {
        GameObject prefab = currentItem.prefab;
        if (prefab.GetComponent<Plant>() == null || !TerrainTilemap.Instance.CheckTilled(currPlace))
        {
            placingCoroutine = null;
            isPlacing = false;
            yield return null;
        }
        player.Planting();

        yield return new WaitUntil(() => player.animFinished);

        //Debug.Log(player.animFinished);
        
        Instantiate(prefab, currPlace, Quaternion.identity);

        inventoryManager.UseThisItem(slot);
        player.animFinished = false;
        placingCoroutine = null;
        isPlacing = false;
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