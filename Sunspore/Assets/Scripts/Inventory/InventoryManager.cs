using Sunspore;
using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    Inventory inventory;
    [SerializeField] Hotbar hotbar;
    [SerializeField] WeaponManager weaponManager;

    void Awake()
    {
        // load inventory from some file
        inventory = new Inventory();
        hotbar.UpdateUI(inventory);
        weaponManager = GetComponent<WeaponManager>();
    }

    void Update()
    {
        // navigate inventory
        if (Input.GetAxis("Mouse ScrollWheel") is float axis)
        {
            if (hotbar.GetSelectedItem(inventory) != null && hotbar.GetSelectedItem(inventory).weapon)
            {
                weaponManager.equipped = false;
            }

            hotbar.ChangeSelected(axis);
            hotbar.UpdateUI(inventory);

            // check if the currenlty selected item is a weapon
            if (hotbar.GetSelectedItem(inventory) != null && hotbar.GetSelectedItem(inventory).weapon)
            {
                weaponManager.equipped = true;
            }
        }
    }

    public InventoryItem GetSelectedItem()
    {
        return hotbar.GetSelectedItem(inventory);
    }

    public void AddItem(InventoryItem item)
    {
        inventory.AddItem(item);
        hotbar.UpdateUI(inventory);
    }
    public bool ContainsItem(InventoryItem item)
    {
        return inventory.ContainsItem(item);
    }

    public void RemoveItem(InventoryItem item)
    {
        inventory.RemoveItem(item);
        hotbar.UpdateUI(inventory);
    }

    public void UseItem(InventoryItem item)
    {
        inventory.UseItem(item);
        hotbar.UpdateUI(inventory);
    }
}