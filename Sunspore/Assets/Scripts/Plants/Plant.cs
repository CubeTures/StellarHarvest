using UnityEngine;

namespace Sunspore
{
    public enum GrowthStage
    {
        Seed,       // Does nothing
        Juvenile,   // Limited abilities
        FullyGrown  // Full abilities
    }

    public abstract class Plant : MonoBehaviour
    {
        [Header("Growth Stages")]
        [SerializeField] protected Sprite seedSprite;
        [SerializeField] protected Sprite juvenileSprite;
        [SerializeField] protected Sprite fullyGrownSprite;

        [Header("Growth Settings")]
        [SerializeField] protected float seedToJuvenileTime = 15f;
        [SerializeField] protected float juvenileToFullyGrownTime = 15f;
        [SerializeField] protected float growthTimeVariance = 5f;

        [Header("Audio")]
        [SerializeField] protected AudioClip harvestSound;
        [SerializeField] protected AudioClip growSound;
        [SerializeField] protected AudioClip plantSound;

        [Header("Effects")]
        [SerializeField] protected GameObject glowEffect;

        // Current growth stage
        protected GrowthStage currentStage = GrowthStage.Seed;

        // Growth tracking
        protected float _growthTimer = 0f;
        protected float _currentGrowthTime;
        protected float _initialSeedToJuvenileTime;
        protected float _initialJuvenileToFullyGrownTime;
        protected float _health = 50f;

        // Components
        protected SpriteRenderer _spriteRenderer;
        protected AudioSource _audioSource;

        protected virtual void Start()
        {
            Pathfinding.Instance.AddPlant(transform, PlantTypes.VALUE);

            _audioSource = GetComponent<AudioSource>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            // Initialize growth times
            _initialSeedToJuvenileTime = seedToJuvenileTime;
            _initialJuvenileToFullyGrownTime = juvenileToFullyGrownTime;

            // Start as seed
            SetGrowthStage(GrowthStage.Seed);
        }

        protected virtual void LateUpdate()
        {
            // Handle growth progression
            if (currentStage == GrowthStage.Seed)
            {
                _growthTimer += Time.deltaTime;
                if (_growthTimer >= _currentGrowthTime)
                {
                    GrowToJuvenile();
                }
            }
            else if (currentStage == GrowthStage.Juvenile)
            {
                _growthTimer += Time.deltaTime;
                if (_growthTimer >= _currentGrowthTime)
                {
                    GrowToFullyGrown();
                }

                // Juvenile abilities update
                UpdateJuvenileAbilities();
            }
            else if (currentStage == GrowthStage.FullyGrown)
            {
                // Fully grown abilities update
                UpdateFullyGrownAbilities();
            }
        }

        protected virtual void SetGrowthStage(GrowthStage stage)
        {
            currentStage = stage;
            _growthTimer = 0f;

            switch (stage)
            {
                case GrowthStage.Seed:
                    _spriteRenderer.sprite = seedSprite;
                    _currentGrowthTime = _initialSeedToJuvenileTime + Random.Range(0f, growthTimeVariance);
                    OnSeedStage();
                    break;

                case GrowthStage.Juvenile:
                    _spriteRenderer.sprite = juvenileSprite;
                    _currentGrowthTime = _initialJuvenileToFullyGrownTime + Random.Range(0f, growthTimeVariance);
                    OnJuvenileStage();
                    break;

                case GrowthStage.FullyGrown:
                    _spriteRenderer.sprite = fullyGrownSprite;
                    OnFullyGrownStage();
                    break;
            }
        }

        protected virtual void GrowToJuvenile()
        {
            PlaySound(growSound);
            SetGrowthStage(GrowthStage.Juvenile);
        }

        protected virtual void GrowToFullyGrown()
        {
            PlaySound(growSound);
            SetGrowthStage(GrowthStage.FullyGrown);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && currentStage == GrowthStage.FullyGrown)
            {
                PlaySound(harvestSound);
                Harvest();
            }
        }

        protected virtual void Harvest()
        {
            SetGrowthStage(GrowthStage.Seed);
            GameManager.gm.UpdateCredits(10);
            Instantiate(glowEffect, transform.position, Quaternion.identity);
        }

        protected void PlaySound(AudioClip sound)
        {
            if (sound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(sound);
            }
        }

        // Virtual methods for child classes to override for stage-specific behavior
        protected virtual void OnSeedStage()
        {
            // Seeds do nothing by default
        }

        protected virtual void OnJuvenileStage()
        {
            // Called when plant enters juvenile stage
        }

        protected virtual void OnFullyGrownStage()
        {
            // Called when plant enters fully grown stage
        }

        protected virtual void UpdateJuvenileAbilities()
        {
            // Override in child classes for limited abilities during juvenile stage
        }

        protected virtual void UpdateFullyGrownAbilities()
        {
            // Override in child classes for full abilities during fully grown stage
        }

        // Public method to get current growth stage
        public GrowthStage GetGrowthStage()
        {
            return currentStage;
        }

        // Public method to check if plant can use abilities
        public bool CanUseJuvenileAbilities()
        {
            return currentStage == GrowthStage.Juvenile || currentStage == GrowthStage.FullyGrown;
        }

        public bool CanUseFullAbilities()
        {
            return currentStage == GrowthStage.FullyGrown;
        }

        public bool TakeDamage(float amount)
        {
            _health -= amount;
            bool dead = _health <= 0;
            if (dead)
            {
                Destroy(gameObject);
            }
            return dead;
        }

        void OnDestroy()
        {
            Pathfinding.Instance.RemovePlant(transform);
        }
    }
}
