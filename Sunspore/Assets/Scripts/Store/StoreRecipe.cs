using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Sunspore
{
    public class StoreRecipe : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public StoreRecipeSO storeSO;

        [SerializeField] GameObject itemIconPrefab;
        [SerializeField] GameObject equalIconPrefab;
        [SerializeField] float iconSpacing;

        private bool canProcess;
        [SerializeField] UnityEngine.UI.Image backgroundImage;
        [SerializeField] Color normalColor = new Color(1f,1f,1f,1f);
        [SerializeField] Color selectedColor = new Color(0.8f,0.9f,1f,1f);

        public void OnPointerEnter(PointerEventData eventData)
        {
            canProcess = StoreManager.Instance != null && StoreManager.Instance.CanProcessRecipe(storeSO);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // no-op
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (StoreManager.Instance != null)
            {
                StoreManager.Instance.SelectRecipe(storeSO, this);
            }
        }

        public void SetSelected(bool sel)
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<UnityEngine.UI.Image>();
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = sel ? selectedColor : normalColor;
            }
        }

        public void UpdateRecipeUI(StoreRecipeSO newStoreSO)
        {
            storeSO = newStoreSO;

            if (storeSO == null || storeSO.ingredients == null || itemIconPrefab == null)
            {
                Debug.LogError("StoreRecipe: Missing references in UpdateRecipeUI.");
                return;
            }

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            var hLayout = GetComponent<HorizontalLayoutGroup>();
            if (hLayout == null)
            {
                hLayout = gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            hLayout.spacing = iconSpacing;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;
            hLayout.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < storeSO.ingredients.Length; i++)
            {
                GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                newItemIcon.transform.GetChild(0).GetComponent<Image>().sprite = storeSO.ingredients[i].item.icon;
                newItemIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = storeSO.ingredients[i].amount.ToString();
            }

            // show credit reward as text instead of result icons
            GameObject rewardTextObj = new GameObject("RewardText", typeof(RectTransform));
            rewardTextObj.transform.SetParent(transform);
            var txt = rewardTextObj.AddComponent<TextMeshProUGUI>();
            txt.text = $"Credits: {storeSO.creditReward}";
            txt.fontSize = 20;
            txt.alignment = TextAlignmentOptions.Center;
        }
    }
}
