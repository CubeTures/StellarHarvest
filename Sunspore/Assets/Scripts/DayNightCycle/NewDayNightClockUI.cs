using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sunspore
{
	public class NewDayNightClockUI : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private DayNightManager dayNightManager;
		[SerializeField] private RectTransform dialImage;
		[SerializeField] private TMP_Text timerText;
		[SerializeField] private Image iconImage;

		[Header("Icons")]
		[SerializeField] private Sprite dayIcon;
		[SerializeField] private Sprite nightIcon;

		void Awake()
		{
			if (dayNightManager == null)
				dayNightManager = FindFirstObjectByType<DayNightManager>();
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
			// Rotate 0->180 during day (0..0.5), 180->360 during night (0.5..1)
			float angle = normalizedTime * 360f + 180;
			dialImage.localEulerAngles = new Vector3(0f, 0f, -angle);

			bool isNight = normalizedTime >= 0.5f;
			float halfCycle = dayNightManager.dayLengthSeconds * 0.5f;

			float secondsLeft;
			if (!isNight)
			{
				secondsLeft = (0.5f - normalizedTime) * dayNightManager.dayLengthSeconds;
				timerText.text = Mathf.CeilToInt(secondsLeft) + " sec left";
				if (iconImage != null && dayIcon != null)
					iconImage.sprite = dayIcon;
			}
			else
			{
				secondsLeft = (1f - normalizedTime) * dayNightManager.dayLengthSeconds;
				timerText.text = Mathf.CeilToInt(secondsLeft) + " sec left";
				if (iconImage != null && nightIcon != null)
					iconImage.sprite = nightIcon;
			}
		}
	}
}
