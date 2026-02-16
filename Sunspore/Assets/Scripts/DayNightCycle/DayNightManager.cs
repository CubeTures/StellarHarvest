using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sunspore
{
    public class DayNightManager : MonoBehaviour
    {
        private Vector3 rotation = Vector3.zero;
        [SerializeField] EnemySpawner spawner;
        [SerializeField] GameObject Light;
        [SerializeField] float dayLengthSeconds = 30f;
        [SerializeField] TMP_Dropdown timeSelector;

        GameManager gm;

        private float degPerSec = 0f;
        private float timeOfDay = 0f;
        private bool timeStopped = false;
        private bool isNight = false;
        private bool isDay = false;

        private int days = 0;
        private int nights = 0;
        private int weeks = 0;
        
        void Start()
        {
            degPerSec = 360f / dayLengthSeconds;
            spawner = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();
            Light = GameObject.Find("Directional Light");
            timeSelector = GameObject.Find("Dropdown").GetComponent<TMP_Dropdown>();
            gm = GameManager.gm;
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

                // Day counter: when timeOfDay wraps around to 0
                if (prevTimeOfDay > timeOfDay)
                {
                    days++;
                    Debug.Log("Days: " + days);
                }

                if (prevTimeOfDay < 0.5f && timeOfDay >= 0.5f)
                {
                    nights++;
                    Debug.Log("Nights: " + nights);
                }

                if (days == 7  && nights == 7)
                {
                    weeks++;
                    Debug.Log("Weeks: " + weeks);
                    days = 0;
                    nights = 0;
                    Bank.Instance.CheckPayment(weeks);
                }
            }

            if (!timeStopped && timeOfDay >= 0.5f || isNight)
            {
                spawner.SpawnEnemy();
                isNight = true;
                isDay = false;
            }
            else
            {
                isDay = true;
                isNight = false;
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
            if(timeStopped && timeSelector.value == 0)
            {
                startTime();
            }
            else if(isDay && timeSelector.value == 1)
            {
                stopTime();
            }
            else if(timeSelector.value == 2)
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
