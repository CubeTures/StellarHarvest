using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sunspore
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Tooltip : MonoBehaviour
    {
        public static Tooltip Instance { get; private set; }

        [SerializeField] TMP_Text headerText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] LayoutElement layoutElement;
        [SerializeField] int maxWidth = 200;
        [SerializeField] Vector2 offset = new(12f, -12f);

        RectTransform _rect;
        Canvas _canvas;
        CanvasGroup _canvasGroup;

        void Awake()
        {
            Instance = this;
            _rect = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        public void Show(string header, string body = "")
        {
            headerText.text = header;

            bool hasBody = !string.IsNullOrEmpty(body);
            bodyText.gameObject.SetActive(hasBody);
            if (hasBody)
                bodyText.text = body;

            // int headerLength = headerText.preferredWidth > maxWidth ? maxWidth : 0;
            // int bodyLength = bodyText.preferredWidth > maxWidth ? maxWidth : 0;
            // layoutElement.enabled = headerLength > 0 || bodyLength > 0;
            // if (layoutElement.enabled)
            //     layoutElement.preferredWidth = maxWidth;

            SetVisible(true);
            UpdatePosition();
        }

        public void Hide()
        {
            SetVisible(false);
        }

        void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        void Update()
        {
            if (_canvasGroup.alpha > 0f)
                UpdatePosition();
        }

        void UpdatePosition()
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 anchoredPos;

            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                anchoredPos = mousePos + offset;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform,
                    mousePos,
                    _canvas.worldCamera,
                    out anchoredPos
                );
                anchoredPos += offset;
            }

            Vector2 size = _rect.sizeDelta;
            if (anchoredPos.x + size.x > Screen.width)
                anchoredPos.x = mousePos.x - offset.x - size.x;
            if (anchoredPos.y - size.y < 0)
                anchoredPos.y = mousePos.y - offset.y + size.y;

            _rect.anchoredPosition = anchoredPos;
        }
    }
}
