using UnityEngine;

public class Merchant : MonoBehaviour
{
    Player player;
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] Wallet wallet;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void Update()
    {
        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist <= 3 && Input.GetKeyDown(KeyCode.Space))
        {
            SellSelectedItem();
        }
    }

    void SellSelectedItem()
    {
        var item = inventoryManager.GetSelectedItem();
        if (item != null)
        {
            inventoryManager.UseSelectedItem();
            if (wallet != null)
            {
                decimal before = wallet.GetAmount();
                wallet.Add(item.sellPrice);
                decimal after = wallet.GetAmount();
                Debug.Log($"[Merchant] Sold item {item.name}. sellPrice={item.sellPrice}, wallet before={before}, wallet after={after}");
            }
            else
            {
                Debug.LogWarning("[Merchant] No Wallet assigned to Merchant.wallet");
            }
        }
    }
}