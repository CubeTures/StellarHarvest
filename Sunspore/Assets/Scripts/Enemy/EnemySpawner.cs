using UnityEngine;

namespace Sunspore
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] GameObject[] enemy;
        [SerializeField] float cooldown = 5f;
        private float lastSpawnTime = -Mathf.Infinity;

        public void SpawnEnemy()
        {
            if(Time.time - lastSpawnTime < cooldown)
            {
                //Debug.Log("Cooldown");
                return;
            }
            //Debug.Log("Enemy Spawned");
            //int randomEnemy = Random.Range(0, enemy.Length);
            Vector3 randomSpawnPoint = new Vector3(Random.Range(0, 10), 0, Random.Range(0, 10));
            Instantiate(enemy[0], randomSpawnPoint, Quaternion.identity);
            lastSpawnTime = Time.time;
        }
    }
}
