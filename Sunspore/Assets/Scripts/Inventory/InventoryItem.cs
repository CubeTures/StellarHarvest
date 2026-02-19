using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Sunspore
{
    public class InventoryItem : MonoBehaviour
    {
        public ItemSO itemScriptableObject;
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text stackText;

        public int stackCurrent = 1;
        public int stackMax;

        private void Start()
        {
            stackMax = itemScriptableObject.maxStackSize;
        }
        void Update()
        {
            iconImage.sprite = itemScriptableObject.icon;

            if (stackMax > 1)
            {
                stackText.text = stackCurrent.ToString();
            }
        }
    }
}
