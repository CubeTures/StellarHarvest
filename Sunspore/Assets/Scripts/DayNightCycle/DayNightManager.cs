using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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

        [Tooltip("While the tutorial has unfinished steps, the day/night clock does not advance.")]
        bool pauseDayNightUntilTutorialComplete = true;

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

        bool IsTutorialBlockingTime()
        {
            if (!pauseDayNightUntilTutorialComplete)
            {
                return false;
            }

            return TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive;
        }

        void Start()
        {
            spawner = GameObject.Find("Enemy Spawner").GetComponent<EnemySpawner>();
            Light = GameObject.Find("Directional Light");
            timeSelector = GameObject.Find("Dropdown").GetComponent<TMP_Dropdown>();
            gm = GameManager.gm;

            // Ensure a deterministic start position for the day/night cycle and clock UI.
            timeOfDay = Mathf.Repeat(initialTimeOfDay, 1f);
            float sunAngle = Mathf.Lerp(0f, 360f, timeOfDay);
            Light.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);
        }

        void Update()
        {
            if (!timeStopped && !IsTutorialBlockingTime())
            {
                // Calculate angle based off percentage of day passed.
                float prevTimeOfDay = timeOfDay;
                timeOfDay += Time.deltaTime / dayLengthSeconds;
                timeOfDay = timeOfDay % 1;

                float sunAngle = Mathf.Lerp(0f, 360f, timeOfDay);
                Light.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

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
                    Bank.Instance.CheckPayment(weeks);
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
