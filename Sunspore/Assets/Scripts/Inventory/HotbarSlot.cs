using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HotbarSlot : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI quantityText;
    [SerializeField] Image ring;

    public void UpdateUI(InventorySlot inv, bool selected)
    {
        if (selected)
        {
            ring.enabled = true;
        }
        else
        {
            ring.enabled = false;
        }

        if (inv == null || inv.IsEmpty)
        {
            icon.enabled = false;
            quantityText.text = "";
        }
        else
        {
            icon.enabled = true;
            icon.sprite = inv.item.icon;
            quantityText.text = inv.item.stackable ? inv.quantity.ToString() : "";
        }
    }
}