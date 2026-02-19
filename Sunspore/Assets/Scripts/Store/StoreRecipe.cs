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
        [SerializeField] Sprite coinSprite;

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

            // If this is a buy recipe, show credits (cost) and/or ingredients -> equal -> results (like crafting)
            if (storeSO.recipeType == StoreRecipeType.buy && storeSO.resultItem != null && storeSO.resultItem.Length > 0 && equalIconPrefab != null)
            {
                // If a creditCost is specified, show it as a coin icon + value (falls back to text if no coin sprite)
                if (storeSO.creditCost > 0)
                {
                    if (itemIconPrefab != null && coinSprite != null)
                    {
                        GameObject costIcon = Instantiate(itemIconPrefab, transform);
                        var img = costIcon.transform.GetChild(0).GetComponent<Image>();
                        var amt = costIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                        if (img != null) img.sprite = coinSprite;
                        if (amt != null) amt.text = storeSO.creditCost.ToString();
                    }
                    else
                    {
                        GameObject costTextObj = new GameObject("CostText", typeof(RectTransform));
                        costTextObj.transform.SetParent(transform);
                        var costTxt = costTextObj.AddComponent<TextMeshProUGUI>();
                        costTxt.text = $"Cost: {storeSO.creditCost}";
                        costTxt.fontSize = 18;
                        costTxt.alignment = TextAlignmentOptions.Center;
                    }
                }

                // show any item ingredients (optional)
                for (int i = 0; i < storeSO.ingredients.Length; i++)
                {
                    GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                    newItemIcon.transform.GetChild(0).GetComponent<Image>().sprite = storeSO.ingredients[i].item.icon;
                    newItemIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = storeSO.ingredients[i].amount.ToString();
                }

                Instantiate(equalIconPrefab, transform);

                for (int i = 0; i < storeSO.resultItem.Length; i++)
                {
                    GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                    newItemIcon.transform.GetChild(0).GetComponent<Image>().sprite = storeSO.resultItem[i].item.icon;
                    newItemIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = storeSO.resultItem[i].amount.ToString();
                }
            }
            else
            {
                // default / sell behavior: show ingredients and credit reward text
                for (int i = 0; i < storeSO.ingredients.Length; i++)
                {
                    GameObject newItemIcon = Instantiate(itemIconPrefab, transform);
                    newItemIcon.transform.GetChild(0).GetComponent<Image>().sprite = storeSO.ingredients[i].item.icon;
                    newItemIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = storeSO.ingredients[i].amount.ToString();
                }

                // insert equal icon before the reward (if available)
                if (equalIconPrefab != null)
                {
                    Instantiate(equalIconPrefab, transform);
                }

                // show credit reward as a coin icon + value (falls back to text if no coin sprite)
                if (storeSO.creditReward > 0)
                {
                    if (itemIconPrefab != null && coinSprite != null)
                    {
                        GameObject rewardIcon = Instantiate(itemIconPrefab, transform);
                        var img = rewardIcon.transform.GetChild(0).GetComponent<Image>();
                        var amt = rewardIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                        if (img != null) img.sprite = coinSprite;
                        if (amt != null) amt.text = storeSO.creditReward.ToString();
                    }
                    else
                    {
                        GameObject rewardTextObj = new GameObject("RewardText", typeof(RectTransform));
                        rewardTextObj.transform.SetParent(transform);
                        var txt = rewardTextObj.AddComponent<TextMeshProUGUI>();
                        txt.text = $"Credits: {storeSO.creditReward}";
                        txt.fontSize = 20;
                        txt.alignment = TextAlignmentOptions.Center;
                    }
                }
            }
        }
    }
}
