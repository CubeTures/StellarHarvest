using UnityEngine;

namespace Sunspore
{
    public class Plant : MonoBehaviour
    {
        public Sprite seedSprite;
        public Sprite plantSprite;
        public float growthTime = 15f;
        float initialGrowthTime;
        public float growthTimeVariance = 5f;
        float _growthTimer = 0f;
        public AudioClip harvestSound;
        public AudioClip growSound;
        public AudioClip plantSound;

        SpriteRenderer _spriteRenderer;
        AudioSource _audioSource;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = seedSprite;
            initialGrowthTime = growthTime;
            growthTime += Random.Range(0f, growthTimeVariance); // Add a random time to the growth time to make crop growth more varied
        }

        // Update is called once per frame
        void LateUpdate()
        {
            // If the plant is a seed, grow it after the growth time
            if (_spriteRenderer.sprite == seedSprite)
            {
                _growthTimer += Time.deltaTime;
                if (_growthTimer >= growthTime)
                {
                    PlaySound(growSound);
                    _spriteRenderer.sprite = plantSprite;
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && _spriteRenderer.sprite == plantSprite)
            {
                PlaySound(harvestSound);
                Harvest();
            }
        }

        public void Harvest()
        {
            _spriteRenderer.sprite = seedSprite;
            _growthTimer = 0f;
            growthTime = initialGrowthTime + Random.Range(0f, growthTimeVariance);
            GameManager.gm.UpdateCredits(10);
        }

        void PlaySound(AudioClip sound)
        {
            _audioSource.PlayOneShot(sound);
        }
    }
}
