using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sunspore
{
    public class DayNightManager : MonoBehaviour
    {
        private Vector3 rotation = Vector3.zero;
        [SerializeField] EnemySpawner spawner;
        [SerializeField] GameObject Light;
        [SerializeField] public float dayLengthSeconds = 30f;
        [SerializeField, Range(0f, 1f)] float initialTimeOfDay = 0f;
        [SerializeField] TMP_Dropdown timeSelector;
        [SerializeField] GameObject nightVolume;
        Volume nightVolumeComponent;
        private Vignette _vignette;
        private WhiteBalance _whiteBalance;

        GameManager gm;

        private float timeOfDay = 0f;
        private bool timeStopped = false;
        private bool isNight = false;
        private bool isDay = false;

        private int days = 0;
        private int nights = 0;
        public int weeks = 0;

        public float NormalizedTimeOfDay => timeOfDay;
        public event Action<float> TimeOfDayChanged;

        void Start()
        {
            spawner = GameObject.Find("Enemy Spawner").GetComponent<EnemySpawner>();
            Light = GameObject.Find("Directional Light");
            // timeSelector = GameObject.Find("Dropdown").GetComponent<TMP_Dropdown>();
            gm = GameManager.gm;

            // Ensure a deterministic start position for the day/night cycle and clock UI.
            timeOfDay = Mathf.Repeat(initialTimeOfDay, 1f);
            float sunAngle = Mathf.Lerp(0f, 360f, timeOfDay);
            Light.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

            if (nightVolume != null)
            {
                nightVolumeComponent = nightVolume.GetComponent<Volume>();
            }
            else
            {
                nightVolumeComponent = GameObject.Find("Night Volume").GetComponent<Volume>();
            }

            if (nightVolumeComponent != null && nightVolumeComponent.profile != null)
            {
                nightVolumeComponent.profile.TryGet(out _vignette);
                nightVolumeComponent.profile.TryGet(out _whiteBalance);
            }
        }

        void Update()
        {
            if (!timeStopped)
            {
                // Calculate angle based off percentage of day passed.
                float prevTimeOfDay = timeOfDay;
                timeOfDay += Time.deltaTime / dayLengthSeconds;
                timeOfDay = timeOfDay % 1;

                float sunAngle = Mathf.Lerp(0f, 360f, timeOfDay);
                Light.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

                // Seamless 0..1 cycle (same value at timeOfDay 0 and 1).
                float nightBlend = (1f - Mathf.Cos(timeOfDay * Mathf.PI * 2f)) * 0.5f;
                if (_vignette != null)
                {
                    _vignette.intensity.value = nightBlend * 0.4f;
                }
                if (_whiteBalance != null)
                {
                    _whiteBalance.temperature.value = nightBlend * -25f;
                }

                // Day counter: when timeOfDay wraps around to 0
                if (prevTimeOfDay > timeOfDay)
                {
                    days++;
                    //Debug.Log("day");
                    spawner.spawning = false;
                    // Debug.Log("Days: " + days);
                }

                if (prevTimeOfDay < 0.5f && timeOfDay >= 0.5f)
                {
                    nights++;
                    //Debug.Log("night");
                    spawner.SetEnemiesPerWave();
                    spawner.spawning = true;
                    // Debug.Log("Nights: " + nights);
                }

                if (days == 7 && nights == 7)
                {
                    weeks++;
                    // Debug.Log("Weeks: " + weeks);
                    days = 0;
                    nights = 0;
                    // Bank.Instance.CheckPayment(weeks);
                }
            }

            TimeOfDayChanged?.Invoke(timeOfDay);

            if (isNight)
            {
                spawner.SetEnemiesPerWave();
                spawner.spawning = true;
            }
        }

        void startTime()
        {
            timeStopped = false;
            isNight = false;
            //Debug.Log("Cycle");
        }

        void stopTime()
        {
            timeStopped = true;
            isNight = false;
        }

        void setDay()
        {
            timeStopped = true;
            isNight = false;
            Light.transform.rotation = Quaternion.Euler(90, 0, 0);
            //Debug.Log("Day");
        }

        void setNight()
        {
            timeStopped = true;
            isNight = true;
            Light.transform.rotation = Quaternion.Euler(270, 0, 0);
            //Debug.Log("Night");
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
