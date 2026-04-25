using UnityEngine;

namespace Sunspore
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject niblet;
        [SerializeField] private GameObject glutton;
        [SerializeField] private GameObject gnasher;
        [SerializeField] private GameObject skybite;

        [Header("Refs")]
        [SerializeField] private DayNightManager dayNightManager;
        [SerializeField] private GameObject player;

        public enum EnemyType { Niblet, Glutton, Gnasher, Skybite }

        [System.Serializable]
        public class GroupMember
        {
            public EnemyType type;
            public int minCount = 1;
            public int maxCount = 1;
            public float spread = 2f;
        }

        [System.Serializable]
        public class EnemyGroupRecipe
        {
            public string name;
            public float chanceWeight = 1f;

            [Tooltip("If true, this recipe is only eligible for the LAST wave of the night.")]
            public bool hardWave = false;

            public GroupMember[] members;
        }

        [Header("Group Recipes")]
        [SerializeField] private EnemyGroupRecipe[] groupRecipes;

        private float totalGroupWeightAll = 1f;
        private float totalGroupWeightNonHard = 1f;

        [Header("Wave Timing")]
        [SerializeField] private float waveJitterSeconds = 0.5f;

        [Header("Waves scale per NIGHT survived")]
        [SerializeField] private int baseWavesPerNight = 3;
        [SerializeField] private float wavesPerNightGrowth = 0.25f;
        [SerializeField] private int maxWavesPerNight = 25;

        private bool spawning = false;
        private float nextWaveTime = -Mathf.Infinity;

        private int wavesThisNight = 0;
        private int waveIndexThisNight = 0; // 0-based

        private void Start()
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");

            CacheRecipeWeights();
        }

        private void Update()
        {
            if (!spawning) return;
            if (Time.time < nextWaveTime) return;

            if (groupRecipes == null || groupRecipes.Length == 0)
                return;

            bool isLastWave = (waveIndexThisNight >= wavesThisNight - 1);

            var recipe = PickWeightedRecipe(allowHard: isLastWave);
            SpawnRecipeGroup(recipe);

            waveIndexThisNight++;
            if (waveIndexThisNight >= wavesThisNight)
            {
                // We’ve spawned all waves for this night; wait until EndNight()/BeginNight() resets it.
                nextWaveTime = Mathf.Infinity;
            }
            else
            {
                ScheduleNextWave(immediate: false);
            }
        }

        public void BeginNight()
        {
            spawning = true;

            wavesThisNight = GetWavesThisNight();
            waveIndexThisNight = 0;

            ScheduleNextWave(immediate: true);
        }

        public void EndNight()
        {
            spawning = false;
            nextWaveTime = -Mathf.Infinity;
            wavesThisNight = 0;
            waveIndexThisNight = 0;
        }

        private int GetWavesThisNight()
        {
            // per-night scaling (swap to TotalDays if you truly want day-based difficulty)
            int n = (dayNightManager != null) ? dayNightManager.TotalNights : 0;

            int waves = Mathf.FloorToInt(baseWavesPerNight + n * wavesPerNightGrowth);
            return Mathf.Clamp(waves, 1, maxWavesPerNight);
        }

        private void ScheduleNextWave(bool immediate)
        {
            float nightSeconds = (dayNightManager != null) ? (dayNightManager.dayLengthSeconds / 2f) : 45f;
            float interval = (wavesThisNight > 0) ? (nightSeconds / wavesThisNight) : nightSeconds;

            float jitter = Random.Range(-waveJitterSeconds, waveJitterSeconds);

            if (immediate)
                nextWaveTime = Time.time + Mathf.Max(0f, jitter);
            else
                nextWaveTime = Time.time + Mathf.Max(0.05f, interval + jitter);
        }

        private void CacheRecipeWeights()
        {
            totalGroupWeightAll = 0f;
            totalGroupWeightNonHard = 0f;

            if (groupRecipes == null || groupRecipes.Length == 0)
            {
                totalGroupWeightAll = 1f;
                totalGroupWeightNonHard = 1f;
                return;
            }

            foreach (var r in groupRecipes)
            {
                if (r == null) continue;

                float w = Mathf.Max(0f, r.chanceWeight);
                totalGroupWeightAll += w;
                if (!r.hardWave) totalGroupWeightNonHard += w;
            }

            if (totalGroupWeightAll <= 0f) totalGroupWeightAll = 1f;
            if (totalGroupWeightNonHard <= 0f) totalGroupWeightNonHard = totalGroupWeightAll; // fallback
        }

        private EnemyGroupRecipe PickWeightedRecipe(bool allowHard)
        {
            // 1) compute total weight of eligible recipes
            float total = 0f;
            for (int i = 0; i < groupRecipes.Length; i++)
            {
                var r = groupRecipes[i];
                if (r == null) continue;
                if (!allowHard && r.hardWave) continue;

                float w = Mathf.Max(0f, r.chanceWeight);
                total += w;
            }

            // If all weights are 0, fall back to uniform random among eligible
            if (total <= 0f)
            {
                // pick any eligible recipe uniformly
                int eligibleCount = 0;
                for (int i = 0; i < groupRecipes.Length; i++)
                {
                    var r = groupRecipes[i];
                    if (r == null) continue;
                    if (!allowHard && r.hardWave) continue;
                    eligibleCount++;
                }
                if (eligibleCount == 0) return null;

                int pick = Random.Range(0, eligibleCount);
                for (int i = 0; i < groupRecipes.Length; i++)
                {
                    var r = groupRecipes[i];
                    if (r == null) continue;
                    if (!allowHard && r.hardWave) continue;

                    if (pick-- == 0) return r;
                }
                return null;
            }

            // 2) weighted roll among eligible recipes
            float roll = Random.Range(0f, total);
            float cum = 0f;

            for (int i = 0; i < groupRecipes.Length; i++)
            {
                var r = groupRecipes[i];
                if (r == null) continue;
                if (!allowHard && r.hardWave) continue;

                float w = Mathf.Max(0f, r.chanceWeight);
                cum += w;

                if (roll <= cum)
                    return r;
            }

            // should never happen, but safe fallback
            return null;
        }

        private GameObject GetPrefab(EnemyType type)
        {
            return type switch
            {
                EnemyType.Niblet => niblet,
                EnemyType.Glutton => glutton,
                EnemyType.Gnasher => gnasher,
                EnemyType.Skybite => skybite,
                _ => niblet
            };
        }

        private Vector3 GetRingSpawnPoint()
        {
            Vector3 center = (player != null) ? player.transform.position : Vector3.zero;
            float innerRadius = 12f;
            float outerRadius = 15f;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));

            float x = center.x + radius * Mathf.Cos(angle);
            float z = center.z + radius * Mathf.Sin(angle);
            return new Vector3(x, 0f, z);
        }

        private void SpawnRecipeGroup(EnemyGroupRecipe recipe)
        {
            if (recipe == null || recipe.members == null) return;
            Debug.Log($"[EnemySpawner] Spawned wave '{recipe.name}' ({waveIndexThisNight + 1}/{wavesThisNight}) hard={recipe.hardWave}");
            Vector3 groupCenter = GetRingSpawnPoint();

            foreach (var m in recipe.members)
            {
                GameObject prefab = GetPrefab(m.type);
                if (prefab == null) continue;

                int count = Random.Range(m.minCount, m.maxCount + 1);

                for (int i = 0; i < count; i++)
                {
                    Vector2 offset = Random.insideUnitCircle * m.spread;
                    Vector3 spawnPoint = groupCenter + new Vector3(offset.x, 0f, offset.y);
                    Instantiate(prefab, spawnPoint, Quaternion.identity);
                }
            }
        }

#if UNITY_EDITOR
        // Helps if you tweak weights in inspector while playing.
        private void OnValidate()
        {
            CacheRecipeWeights();
        }
#endif
    }
}