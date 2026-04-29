using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sunspore
{
    public class DayNightManager : MonoBehaviour
    {
        public static DayNightManager Instance;
        private Vector3 rotation = Vector3.zero;

        [SerializeField] EnemySpawner spawner;
        [SerializeField] MusicManager musicManager;
        [SerializeField] GameObject Light;
        [SerializeField] public float dayLengthSeconds = 180f;
        [SerializeField, Range(0f, 1f)] float initialTimeOfDay = 0f;
        [SerializeField] TMP_Dropdown timeSelector;
        [SerializeField] GameObject nightVolume;
        [SerializeField] GameObject announcementObject;
        [SerializeField] TMP_Text announcementTitle;
        [SerializeField] TMP_Text announcementSubtitle;
        [SerializeField] float announcementDuration = 3f;
        [SerializeField] float typewriterSpeed = 20f;

        private int totalDays = 0;
        private int totalNights = 0;

        public int TotalDays => totalDays;
        public int TotalNights => totalNights;

        Volume nightVolumeComponent;
        private Vignette _vignette;
        private WhiteBalance _whiteBalance;

        GameManager gm;
        private float timeOfDay = 0f;
        public bool timeStopped = false;

        private bool isNight = false;
        private bool isDay = false;

        private int days = 0;
        private int nights = 0;
        public int weeks = 0;

        public float NormalizedTimeOfDay => timeOfDay;
        public event Action<float> TimeOfDayChanged;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            spawner = GameObject.Find("Enemy Spawner").GetComponent<EnemySpawner>();
            Light = GameObject.Find("Directional Light");
            gm = GameManager.gm;

            timeOfDay = Mathf.Repeat(initialTimeOfDay, 1f);
            float sunAngle = Mathf.Lerp(0f, 360f, timeOfDay);
            Light.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

            if (nightVolume != null)
                nightVolumeComponent = nightVolume.GetComponent<Volume>();
            else
                nightVolumeComponent = GameObject.Find("Night Volume").GetComponent<Volume>();

            if (nightVolumeComponent != null && nightVolumeComponent.profile != null)
            {
                nightVolumeComponent.profile.TryGet(out _vignette);
                nightVolumeComponent.profile.TryGet(out _whiteBalance);
            }

            if (TutorialManager.Instance != null)
                stopTime();

            if (musicManager != null)
            {
                if (timeOfDay < 0.5f) musicManager.PlayDayMusic();
                else musicManager.PlayNightMusic();
            }
        }

        void Update()
        {
            if (!timeStopped)
            {
                float prevTimeOfDay = timeOfDay;

                timeOfDay += Time.deltaTime / dayLengthSeconds;
                timeOfDay = timeOfDay % 1;

                float sunAngle = Mathf.Lerp(0f, 360f, timeOfDay);
                Light.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

                float nightBlend = (1f - Mathf.Cos(timeOfDay * Mathf.PI * 2f)) * 0.5f;

                if (_vignette != null) _vignette.intensity.value = nightBlend * 0.4f;
                if (_whiteBalance != null) _whiteBalance.temperature.value = nightBlend * -50f;

                // Day starts when timeOfDay wraps around
                if (prevTimeOfDay > timeOfDay)
                {
                    days++;
                    totalDays++;

                    spawner.EndNight();
                    if (Pathfinding.Instance != null)
                        Pathfinding.Instance.KillAllEnemies();

                    if (musicManager != null) musicManager.PlayDayMusic();
                    ShowAnnouncement("Day " + totalDays);
                }

                // Night starts at 0.5
                if (prevTimeOfDay < 0.5f && timeOfDay >= 0.5f)
                {
                    nights++;
                    totalNights++;

                    spawner.BeginNight();

                    if (musicManager != null) musicManager.PlayNightMusic();
                    ShowAnnouncement("Night " + totalNights);
                }

                if (days == 7 && nights == 7)
                {
                    weeks++;
                    days = 0;
                    nights = 0;
                }
            }

            TimeOfDayChanged?.Invoke(timeOfDay);
        }

        public void startTime()
        {
            timeStopped = false;
            isNight = false;
            StartCoroutine(DeferredStartAnnouncement());
        }

        IEnumerator DeferredStartAnnouncement()
        {
            float timeout = 2f;
            float waited = 0f;
            DebtManager debt = null;

            while (debt == null || debt.RemainingPayments == 0)
            {
                yield return null;
                waited += Time.deltaTime;
                debt = DebtManager.Instance ?? FindFirstObjectByType<DebtManager>(FindObjectsInactive.Include);
                if (waited >= timeout) break;
            }

            if (timeOfDay < 0.5f)
            {
                totalDays++;
                ShowAnnouncement("Day " + totalDays);
            }
            else
            {
                totalNights++;
                ShowAnnouncement("Night " + totalNights);
            }
        }

        public void stopTime()
        {
            timeStopped = true;
            isNight = false;
            spawner.EndNight();
        }

        public void setDay()
        {
            timeStopped = true;
            isNight = false;
            Light.transform.rotation = Quaternion.Euler(90, 0, 0);
            spawner.EndNight();
            if (Pathfinding.Instance != null)
                Pathfinding.Instance.KillAllEnemies();
        }

        public void setNight()
        {
            timeStopped = true;
            isNight = true;
            Light.transform.rotation = Quaternion.Euler(270, 0, 0);
            spawner.BeginNight();
        }

        private Coroutine _announcementCoroutine;

        void ShowAnnouncement(string title)
        {
            if (announcementObject == null || announcementTitle == null) return;

            string subtitle = "";
            var debt = DebtManager.Instance ?? FindFirstObjectByType<DebtManager>(FindObjectsInactive.Include);

            if (debt != null)
            {
                int remaining = debt.RemainingPayments;
                if (remaining > 0)
                {
                    int daysLeft = debt.GetDaysLeftUntilNextPayment();
                    if (daysLeft <= 0)
                        subtitle = "Next payment due today";
                    else if (daysLeft == 1)
                        subtitle = "1 day until next payment";
                    else
                        subtitle = daysLeft + " days until next payment";
                }
                else
                {
                    subtitle = "All payments complete";
                }
            }

            if (_announcementCoroutine != null)
                StopCoroutine(_announcementCoroutine);

            _announcementCoroutine = StartCoroutine(AnnouncementCoroutine(title, subtitle));
        }

        IEnumerator AnnouncementCoroutine(string title, string subtitle)
        {
            announcementObject.SetActive(true);
            announcementTitle.text = "";
            if (announcementSubtitle != null) announcementSubtitle.text = "";

            var anim = announcementObject.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Enter");

            yield return StartCoroutine(TypewriterCoroutine(announcementTitle, title));

            if (announcementSubtitle != null)
                yield return StartCoroutine(TypewriterCoroutine(announcementSubtitle, subtitle));

            yield return new WaitForSeconds(announcementDuration);
            announcementObject.SetActive(false);
            _announcementCoroutine = null;
        }

        IEnumerator TypewriterCoroutine(TMP_Text label, string fullText)
        {
            label.text = "";
            float delay = typewriterSpeed > 0f ? 1f / typewriterSpeed : 0f;

            foreach (char c in fullText)
            {
                label.text += c;
                yield return new WaitForSeconds(delay);
            }
        }

        public void changeTime()
        {
            if (timeStopped && timeSelector.value == 0)
            {
                startTime();
            }
            else if (isDay && timeSelector.value == 1)
            {
                stopTime();
            }
            else if (timeSelector.value == 2)
            {
                setDay();
            }
            else
            {
                setNight();
            }
        }
    }
}