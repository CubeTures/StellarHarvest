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
        private float degPerSec = 0f;
        private bool stopTime = false;
        private bool setNightTime = false;

        void Start()
        {
            degPerSec = 360f / dayLengthSeconds;
            spawner = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();
            Light = GameObject.Find("Directional Light");
            timeSelector = GameObject.Find("Dropdown").GetComponent<TMP_Dropdown>();
        }
        // Update is called once per frame
        void Update()
        {
            if (!stopTime)
            {
                rotation.x = degPerSec * Time.deltaTime;
                Light.transform.Rotate(rotation, Space.World);
            }
            if (!stopTime && Time.time % dayLengthSeconds >= dayLengthSeconds / 2 || setNightTime)
            {
                //Debug.Log("Attempting Spawning Enemy");
                spawner.SpawnEnemy();
            }
        }

        void startTime()
        {
            stopTime = false;
            setNightTime = false;
            //Debug.Log("Cycle");
        }

        void setDay()
        {
            stopTime = true;
            setNightTime = false;
            Light.transform.rotation = Quaternion.Euler(90, 0, 0);
            //Debug.Log("Day");
        }

        void setNight()
        {
            stopTime = true;
            setNightTime = true;
            Light.transform.rotation = Quaternion.Euler(270, 0, 0);
            //Debug.Log("Night");
        }

        public void changeTime()
        {
            if(timeSelector.value == 0)
            {
                startTime();
            }
            else if(timeSelector.value == 1)
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
