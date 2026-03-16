using UnityEngine;

namespace Sunspore
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] GameObject[] enemy;
        [SerializeField] DayNightManager dayNightManager;
        [SerializeField] GameObject player;
        private float lastSpawnTime = -Mathf.Infinity;

        private int baseWaveAmount = 5;
        [SerializeField] private float enemiesPerSecond = 0.5f;
        [SerializeField] private int waveAmount = 0;
        private float difficultyScale = 0.75f;
        public bool spawning = false;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            enemiesPerSecond = baseWaveAmount / (dayNightManager.dayLengthSeconds / 2);
            waveAmount = baseWaveAmount;
        }

        void Update()
        {
            if(!spawning || Time.time - lastSpawnTime < 1f / enemiesPerSecond || waveAmount <= 0)
            {
                return;
            }

            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            // Center of the ring
            Vector3 center = player.transform.position; // Adjust as needed

            // Ring radii
            float innerRadius = 12f;
            float outerRadius = 15f;

            // Random angle (in radians)
            float angle = Random.Range(0f, Mathf.PI * 2);

            // Random radius within the ring
            float radius = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));

            // Calculate position
            float x = center.x + radius * Mathf.Cos(angle);
            float z = center.z + radius * Mathf.Sin(angle);
            Vector3 randomSpawnPoint = new Vector3(x, 0, z);

            //int randomEnemy = Random.Range(0, enemy.Length);
            var prefab = enemy[Random.Range(0, enemy.Length)];
            Instantiate(prefab, randomSpawnPoint, Quaternion.identity);
            lastSpawnTime = Time.time;
            waveAmount -= prefab.GetComponent<Enemy>().cost;
        }

        public void SetEnemiesPerWave()
        {
            waveAmount = Mathf.RoundToInt(baseWaveAmount * Mathf.Pow(dayNightManager.weeks + 1, difficultyScale));
            enemiesPerSecond = waveAmount / (dayNightManager.dayLengthSeconds / 2);
        }
    }
}
