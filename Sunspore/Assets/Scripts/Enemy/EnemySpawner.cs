using UnityEngine;

namespace Sunspore
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] GameObject niblet;
        [SerializeField] GameObject glutton;
        [SerializeField] GameObject gnasher;
        [SerializeField] GameObject skybite;

        private GameObject[] enemies;

        [SerializeField] DayNightManager dayNightManager;
        [SerializeField] GameObject player;
        private float lastSpawnTime = -Mathf.Infinity;

        private int baseWaveAmount = 5;
        [SerializeField] private float enemiesPerSecond = 0.5f;
        [SerializeField] private int waveAmount = 0;
        private float difficultyScale = 0.75f;
        public bool spawning = false;

        [Header("Group Spawning")]
        [SerializeField, Range(0f, 1f)] private float groupSpawnChance = 0.3f;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            enemies = new GameObject[] { niblet, glutton, gnasher, skybite };
            enemiesPerSecond = baseWaveAmount / (dayNightManager.dayLengthSeconds / 2);
            waveAmount = baseWaveAmount;
        }

        void Update()
        {
            if (!spawning || Time.time - lastSpawnTime < 1f / enemiesPerSecond || waveAmount <= 0)
            {
                return;
            }

            if (Random.value < groupSpawnChance)
                SpawnGroup();
            else
                SpawnEnemy();
        }

        private Vector3 GetRingSpawnPoint()
        {
            Vector3 center = player.transform.position;
            float innerRadius = 12f;
            float outerRadius = 15f;

            float angle = Random.Range(0f, Mathf.PI * 2);
            float radius = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));

            float x = center.x + radius * Mathf.Cos(angle);
            float z = center.z + radius * Mathf.Sin(angle);
            return new Vector3(x, 0, z);
        }

        private void SpawnEnemy()
        {
            Vector3 spawnPoint = GetRingSpawnPoint();
            var prefab = enemies[Random.Range(0, enemies.Length)];
            Instantiate(prefab, spawnPoint, Quaternion.identity);
            lastSpawnTime = Time.time;
            waveAmount -= prefab.GetComponent<Enemy>().cost;
        }

        private void SpawnGroup()
        {
            var prefab = enemies[Random.Range(0, enemies.Length)];
            var stats = prefab.GetComponent<Enemy>();

            int scaledMax = Mathf.RoundToInt(stats.maxGroupSize + (dayNightManager.weeks * stats.groupSizeScale));
            int groupSize = Random.Range(stats.minGroupSize, scaledMax + 1);

            Vector3 groupCenter = GetRingSpawnPoint();

            for (int i = 0; i < groupSize && waveAmount > 0; i++)
            {
                Vector2 offset = Random.insideUnitCircle * stats.groupSpread;
                Vector3 spawnPoint = groupCenter + new Vector3(offset.x, 0, offset.y);

                Instantiate(prefab, spawnPoint, Quaternion.identity);
                waveAmount -= stats.cost;
            }

            lastSpawnTime = Time.time;
        }

        public void SetEnemiesPerWave()
        {
            waveAmount = Mathf.RoundToInt(baseWaveAmount * Mathf.Pow(dayNightManager.weeks + 1, difficultyScale));
            enemiesPerSecond = waveAmount / (dayNightManager.dayLengthSeconds / 2);
        }
    }
}
