using UnityEngine;

namespace Sunspore
{
    public class DayNightClockUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DayNightManager dayNightManager;
        [SerializeField] private RectTransform imageA;
        [SerializeField] private RectTransform imageB;

        private float _imageWidth;
        private float _startX;

        void Awake()
        {
            if (dayNightManager == null)
                dayNightManager = FindFirstObjectByType<DayNightManager>();

            _imageWidth = imageA.rect.width;
            _startX = imageA.anchoredPosition.x;

            imageB.anchoredPosition = new Vector2(_startX + _imageWidth, imageB.anchoredPosition.y);
        }

        void OnEnable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.TimeOfDayChanged += HandleTimeChanged;
                HandleTimeChanged(dayNightManager.NormalizedTimeOfDay);
            }
        }

        void OnDisable()
        {
            if (dayNightManager != null)
                dayNightManager.TimeOfDayChanged -= HandleTimeChanged;
        }

        private void HandleTimeChanged(float normalizedTime)
        {
            float offset = normalizedTime * _imageWidth;

            imageA.anchoredPosition = new Vector2(_startX - offset, imageA.anchoredPosition.y);
            imageB.anchoredPosition = new Vector2(_startX + _imageWidth - offset, imageB.anchoredPosition.y);
        }
    }
}