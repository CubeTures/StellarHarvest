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

        private float totalSpawnWeight;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            enemies = new GameObject[] { niblet, glutton, gnasher, skybite };
            enemiesPerSecond = baseWaveAmount / (dayNightManager.dayLengthSeconds / 2);
            waveAmount = baseWaveAmount;
            CacheTotalSpawnWeight();
        }

        private void CacheTotalSpawnWeight()
        {
            totalSpawnWeight = 0f;
            foreach (var enemy in enemies)
                totalSpawnWeight += enemy.GetComponent<Enemy>().spawnChance;
        }

        void Update()
        {
            if (!spawning || Time.time - lastSpawnTime < 1f / enemiesPerSecond || waveAmount <= 0)
            {
                return;
            }

            var prefab = PickWeightedEnemy();
            var stats = prefab.GetComponent<Enemy>();

            if (Random.value < stats.groupSpawnChance)
                SpawnGroup(prefab, stats);
            else
                SpawnEnemy(prefab, stats);
        }

        private GameObject PickWeightedEnemy()
        {
            float roll = Random.Range(0f, totalSpawnWeight);
            float cumulative = 0f;
            foreach (var enemy in enemies)
            {
                cumulative += enemy.GetComponent<Enemy>().spawnChance;
                if (roll <= cumulative)
                    return enemy;
            }
            return enemies[enemies.Length - 1];
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

        private void SpawnEnemy(GameObject prefab, Enemy stats)
        {
            Vector3 spawnPoint = GetRingSpawnPoint();
            Instantiate(prefab, spawnPoint, Quaternion.identity);
            lastSpawnTime = Time.time;
            waveAmount -= stats.cost;
        }

        private void SpawnGroup(GameObject prefab, Enemy stats)
        {
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
