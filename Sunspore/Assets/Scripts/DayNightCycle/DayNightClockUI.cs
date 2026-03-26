using TMPro;
using UnityEngine;

namespace Sunspore
{
    public class DayNightClockUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] DayNightManager dayNightManager;
        [SerializeField] TMP_Text clockLabel;
        // Mask/window rect used as fallback movement range if bounds are not assigned.
        [SerializeField] RectTransform arrowTrack;
        [SerializeField] RectTransform movingBar;
        [SerializeField] RectTransform barStartBound;
        [SerializeField] RectTransform barEndBound;

        [Header("Clock")]
        [SerializeField] bool showClockLabel = false;
        [SerializeField] bool use12HourFormat = true;
        [SerializeField, Range(0, 23)] int dayStartHour = 6;

        [Header("Movement")]
        [SerializeField] Vector2 barPadding = new Vector2(12f, 12f);
        [SerializeField] bool reverseDirection = false;
        [SerializeField, Range(-1f, 1f)] float timelineOffsetNormalized = 0f;

        private bool warnedMissingRefs;
        private float lastNormalizedTime = -1f;
        private float stagnantTimeSeconds;
        private bool warnedTimeNotAdvancing;

        void Awake()
        {
            if (dayNightManager == null)
            {
                dayNightManager = FindFirstObjectByType<DayNightManager>();
            }

            AutoAssignReferences();
        }

        void OnEnable()
        {
            AutoAssignReferences();
            WarnIfMissingReferences();

            if (dayNightManager != null)
            {
                dayNightManager.TimeOfDayChanged += HandleTimeChanged;
                HandleTimeChanged(dayNightManager.NormalizedTimeOfDay);
                lastNormalizedTime = dayNightManager.NormalizedTimeOfDay;
                stagnantTimeSeconds = 0f;
                warnedTimeNotAdvancing = false;
            }
        }

        void Update()
        {
            // Fallback: keep UI synced even if the event was not raised/subscribed.
            if (dayNightManager == null)
            {
                return;
            }

            float normalizedTime = dayNightManager.NormalizedTimeOfDay;
            HandleTimeChanged(normalizedTime);

            if (Mathf.Abs(normalizedTime - lastNormalizedTime) < 0.0001f)
            {
                stagnantTimeSeconds += Time.unscaledDeltaTime;
                if (!warnedTimeNotAdvancing && stagnantTimeSeconds >= 2f)
                {
                    warnedTimeNotAdvancing = true;
                    Debug.LogWarning("DayNightClockUI: time is not advancing. Ensure the day/night dropdown is set to Cycle.", this);
                }
            }
            else
            {
                stagnantTimeSeconds = 0f;
                warnedTimeNotAdvancing = false;
            }

            lastNormalizedTime = normalizedTime;
        }

        void OnDisable()
        {
            if (dayNightManager != null)
            {
                dayNightManager.TimeOfDayChanged -= HandleTimeChanged;
            }
        }

        private void HandleTimeChanged(float normalizedTime)
        {
            normalizedTime = Mathf.Repeat(normalizedTime, 1f);
            float visualTime = Mathf.Repeat(normalizedTime + timelineOffsetNormalized, 1f);

            if (showClockLabel)
            {
                UpdateClockText(normalizedTime);
            }
            else if (clockLabel != null)
            {
                clockLabel.text = string.Empty;
            }

            UpdateBarPosition(visualTime);
        }

        private void UpdateClockText(float normalizedTime)
        {
            if (clockLabel == null)
            {
                return;
            }

            int totalMinutes = Mathf.FloorToInt(normalizedTime * 24f * 60f);
            totalMinutes = (totalMinutes + dayStartHour * 60) % (24 * 60);

            int hour24 = totalMinutes / 60;
            int minute = totalMinutes % 60;

            if (use12HourFormat)
            {
                int hour12 = hour24 % 12;
                if (hour12 == 0)
                {
                    hour12 = 12;
                }

                string amPm = hour24 >= 12 ? "PM" : "AM";
                clockLabel.text = string.Format("{0:00}:{1:00} {2}", hour12, minute, amPm);
            }
            else
            {
                clockLabel.text = string.Format("{0:00}:{1:00}", hour24, minute);
            }
        }

        private void UpdateBarPosition(float normalizedTime)
        {
            if (movingBar == null)
            {
                WarnIfMissingReferences();
                return;
            }

            RectTransform barParent = movingBar.parent as RectTransform;
            if (barParent == null)
            {
                return;
            }

            float minX;
            float maxX;

            if (barStartBound != null && barEndBound != null)
            {
                minX = GetXInParentSpace(barStartBound, barParent);
                maxX = GetXInParentSpace(barEndBound, barParent);
            }
            else if (arrowTrack != null)
            {
                Vector3[] corners = new Vector3[4];
                arrowTrack.GetWorldCorners(corners);
                minX = barParent.InverseTransformPoint(corners[0]).x + barPadding.x;
                maxX = barParent.InverseTransformPoint(corners[3]).x - barPadding.y;
            }
            else
            {
                WarnIfMissingReferences();
                return;
            }

            if (reverseDirection)
            {
                (minX, maxX) = (maxX, minX);
            }

            Vector2 newPos = movingBar.anchoredPosition;
            newPos.x = Mathf.Lerp(minX, maxX, normalizedTime);
            movingBar.anchoredPosition = newPos;
        }

        private static float GetXInParentSpace(RectTransform source, RectTransform parent)
        {
            return parent.InverseTransformPoint(source.position).x;
        }

        private void AutoAssignReferences()
        {
            if (movingBar == null)
            {
                Transform found = transform.Find("ClockWindow/MovingBar");
                if (found != null)
                {
                    movingBar = found as RectTransform;
                }
            }

            if (arrowTrack == null)
            {
                Transform found = transform.Find("ClockWindow");
                if (found != null)
                {
                    arrowTrack = found as RectTransform;
                }
            }

            if (barStartBound == null)
            {
                Transform found = transform.Find("ClockWindow/BarStartBound");
                if (found != null)
                {
                    barStartBound = found as RectTransform;
                }
            }

            if (barEndBound == null)
            {
                Transform found = transform.Find("ClockWindow/BarEndBound");
                if (found != null)
                {
                    barEndBound = found as RectTransform;
                }
            }
        }

        private void WarnIfMissingReferences()
        {
            if (warnedMissingRefs)
            {
                return;
            }

            if (dayNightManager == null || movingBar == null || (barStartBound == null && arrowTrack == null) || (barEndBound == null && arrowTrack == null))
            {
                warnedMissingRefs = true;
                Debug.LogWarning("DayNightClockUI: missing references. Assign DayNightManager, MovingBar, and either both BarStartBound/BarEndBound or ArrowTrack (ClockWindow).", this);
            }
        }

        void OnValidate()
        {
            AutoAssignReferences();
        }
    }
}
