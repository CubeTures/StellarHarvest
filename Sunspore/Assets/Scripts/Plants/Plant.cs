using UnityEngine;

namespace Sunspore
{
    public class Plant : MonoBehaviour
    {
        [SerializeField] Sprite seedSprite;
        [SerializeField] Sprite growingSprite;
        [SerializeField] Sprite plantSprite;
        [SerializeField] float growthTime = 15f;
        float initialGrowthTime;
        [SerializeField] float growthTimeVariance = 5f;
        float _growthTimer = 0f;
        [SerializeField] AudioClip harvestSound;
        [SerializeField] AudioClip growSound;
        [SerializeField] AudioClip plantSound;
        [SerializeField] GameObject glowEffect;

        SpriteRenderer _spriteRenderer;
        AudioSource _audioSource;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Pathfinding.Instance.AddPlant(transform, PlantTypes.VALUE);

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
                    _spriteRenderer.sprite = growingSprite;
                    growthTime = initialGrowthTime + Random.Range(0f, growthTimeVariance); // Reset growthTime
                    _growthTimer = 0f; // Reset growthTimer
                }
            }

            // If the plant is growing, grow it after the growth time
            if (_spriteRenderer.sprite == growingSprite)
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

        void Harvest()
        {
            _spriteRenderer.sprite = seedSprite;
            _growthTimer = 0f;
            growthTime = initialGrowthTime + Random.Range(0f, growthTimeVariance);
            GameManager.gm.UpdateCredits(10);
            Instantiate(glowEffect, transform.position, Quaternion.identity);
        }

        void PlaySound(AudioClip sound)
        {
            _audioSource.PlayOneShot(sound);
        }

        void OnDestroy()
        {
            Pathfinding.Instance.RemovePlant(transform);
        }
    }
}
