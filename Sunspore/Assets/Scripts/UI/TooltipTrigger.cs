using UnityEngine;
using UnityEngine.EventSystems;

namespace Sunspore
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] string header;
        [SerializeField, TextArea] string body;

        public void SetContent(string header, string body = "")
        {
            this.header = header;
            this.body = body;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(header)) return;
            if (Tooltip.Instance != null)
                Tooltip.Instance.Show(header, body);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Tooltip.Instance != null)
                Tooltip.Instance.Hide();
        }

        void OnDisable()
        {
            if (Tooltip.Instance != null)
                Tooltip.Instance.Hide();
        }
    }
}
