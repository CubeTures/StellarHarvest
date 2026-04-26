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
			float angle = normalizedTime * 360f + 180;
			dialImage.localEulerAngles = new Vector3(0f, 0f, -angle);

			bool isNight = normalizedTime >= 0.5f;

			float secondsLeft;
			int cycleNumber;
			string cycleLabel;

			if (!isNight)
			{
				secondsLeft = (0.5f - normalizedTime) * dayNightManager.dayLengthSeconds;
				cycleNumber = dayNightManager.TotalDays;
				cycleLabel = "Day";
				if (iconImage != null && dayIcon != null)
					iconImage.sprite = dayIcon;
			}
			else
			{
				secondsLeft = (1f - normalizedTime) * dayNightManager.dayLengthSeconds;
				cycleNumber = dayNightManager.TotalNights;
				cycleLabel = "Night";
				if (iconImage != null && nightIcon != null)
					iconImage.sprite = nightIcon;
			}

			timerText.text = $"Cycle {cycleNumber}: {cycleLabel}\n{Mathf.CeilToInt(secondsLeft)} sec left";
		}
	}
}