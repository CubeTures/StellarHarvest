using UnityEngine;
using System.Collections;
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
        [Header("Meta")]
        [SerializeField] public PlantTypes plantType = PlantTypes.VALUE;

        [Header("Growth Stages")]
        [SerializeField] protected Sprite seedSprite;
        [SerializeField] protected Sprite juvenileSprite;
        [SerializeField] protected Sprite fullyGrownSprite;

        [Header("Growth Settings")]
        [SerializeField] protected float seedToJuvenileTime = 15f;
        [SerializeField] protected float juvenileToFullyGrownTime = 15f;
        [SerializeField] protected float growthTimeVariance = 5f;
        [SerializeField] protected bool watered = false;

        [Header("Audio")]
        [SerializeField] protected AudioClip harvestSound;
        [SerializeField] protected AudioClip growSound;
        [SerializeField] protected AudioClip plantSound;

        [Header("Effects")]
        [SerializeField] protected GameObject glowEffect;
        [SerializeField] protected GameObject waterDropPrefab;
        private GameObject waterDropInstance;

        [Header("Items")]
        [SerializeField] protected ItemSO seedItem;
        [SerializeField] protected ItemSO plantItem;
        [SerializeField] protected float _transitionTimer = 0f;

        // Current growth stage
        public GrowthStage currentStage = GrowthStage.Seed;

        // Growth tracking
        protected float _growthTimer = 0f;
        protected float _currentGrowthTime;
        protected float _initialSeedToJuvenileTime;
        protected float _initialJuvenileToFullyGrownTime;
        protected Animator _anim;
        protected float _health = 50f;
        protected Pathfinding pathfinding;
        public bool transitionFinish;

        // Components
        protected SpriteRenderer _spriteRenderer;
        protected AudioSource _audioSource;

        protected virtual void Start()
        {
            pathfinding = Pathfinding.Instance;
            pathfinding.AddPlant(this);

            _audioSource = GetComponent<AudioSource>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();

            // Initialize growth times
            _initialSeedToJuvenileTime = seedToJuvenileTime;
            _initialJuvenileToFullyGrownTime = juvenileToFullyGrownTime;

            // Instantiate waterDrop as a child and hide it
            if (waterDropPrefab != null)
            {
                waterDropInstance = Instantiate(waterDropPrefab, transform);
                waterDropInstance.SetActive(true);
            }

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
                    if (watered)
                    {
                        GrowToJuvenile();
                    }
                }
            }
            else if (currentStage == GrowthStage.Juvenile)
            {
                _growthTimer += Time.deltaTime;
                if (_growthTimer >= _currentGrowthTime)
                {
                    if (watered)
                    {
                        GrowToFullyGrown();
                    }
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
                    waterDropInstance.SetActive(true);
                    break;

                case GrowthStage.Juvenile:
                    _spriteRenderer.sprite = juvenileSprite;
                    _currentGrowthTime = _initialJuvenileToFullyGrownTime + Random.Range(0f, growthTimeVariance);
                    OnJuvenileStage();
                    waterDropInstance.SetActive(true);
                    break;

                case GrowthStage.FullyGrown:
                    _spriteRenderer.sprite = fullyGrownSprite;
                    OnFullyGrownStage();
                    break;
            }
        }

        protected virtual void GrowToJuvenile()
        {
            _anim.SetTrigger("Start");
            StartCoroutine(TransitionTimer(GrowthStage.Juvenile));
            watered = false;
        }

        protected virtual void GrowToFullyGrown()
        {
            _anim.SetTrigger("Start");
            StartCoroutine(TransitionTimer(GrowthStage.FullyGrown));
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && currentStage == GrowthStage.FullyGrown)
            {
                PlaySound(harvestSound);
                Harvest();
            }
        }

        public virtual void Harvest()
        {
            if (currentStage == GrowthStage.FullyGrown)
            {
                SetGrowthStage(GrowthStage.Seed);
                InventoryManager.Instance.AddItem(plantItem, 1);
                Instantiate(glowEffect, transform.position, Quaternion.identity);
            }
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

        public void WaterPlant()
        {
            watered = true;
            waterDropInstance.SetActive(false);
        }

        void OnDestroy()
        {
            Pathfinding.Instance.RemovePlant(this);
        }

        private IEnumerator TransitionTimer(GrowthStage growthStage)
        {
            yield return new WaitForSeconds(_transitionTimer);
            _anim.SetTrigger("End");
            PlaySound(growSound);
            SetGrowthStage(growthStage);
        }
    }
}
