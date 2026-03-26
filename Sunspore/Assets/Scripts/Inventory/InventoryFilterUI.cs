using UnityEngine;
using UnityEngine.UI;

namespace Sunspore
{
    // Attach to a UI GameObject that holds the inventory category buttons.
    // Assign the Buttons in the inspector or leave empty to wire by name at Start.
    public class InventoryFilterUI : MonoBehaviour
    {
        public Button buttonAll;
        public Button buttonWeapons;
        public Button buttonSeeds;
        public Button buttonPlants;
        public Button buttonTools;

        void Start()
        {
            // Safe-guard: only wire if InventoryManager exists
            if (InventoryManager.Instance == null) return;

            if (buttonAll != null)
                buttonAll.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_All);
            if (buttonWeapons != null)
                buttonWeapons.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_Weapons);
            if (buttonSeeds != null)
                buttonSeeds.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_Seeds);
            if (buttonPlants != null)
                buttonPlants.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_Plants);
            if (buttonTools != null)
                buttonTools.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_Tools);

            // Optional: Try to auto-find buttons by child name when fields are empty
            if (buttonAll == null) buttonAll = FindButtonByName("All");
            if (buttonWeapons == null) buttonWeapons = FindButtonByName("Weapons");
            if (buttonSeeds == null) buttonSeeds = FindButtonByName("Seeds");
            if (buttonPlants == null) buttonPlants = FindButtonByName("Plants");
            if (buttonTools == null) buttonTools = FindButtonByName("Tools");

            // Add listeners for auto-found buttons (if not already wired)
            if (buttonAll != null) buttonAll.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_All);
            if (buttonWeapons != null) buttonWeapons.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_Weapons);
            if (buttonSeeds != null) buttonSeeds.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_Seeds);
            if (buttonPlants != null) buttonPlants.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_Plants);
            if (buttonTools != null) buttonTools.onClick.AddListener(InventoryManager.Instance.SelectedInventoryCategory_Tools);
        }

        private Button FindButtonByName(string name)
        {
            var t = transform.Find(name);
            if (t == null) return null;
            return t.GetComponent<Button>();
        }
    }
}
