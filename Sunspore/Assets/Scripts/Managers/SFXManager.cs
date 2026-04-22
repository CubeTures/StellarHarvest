using UnityEngine;

namespace Sunspore
{
    public class SFXManager : MonoBehaviour
    {
        public static SFXManager _sfxManager;
        AudioSource _audioSource;

        [Header("SFX")]
        [Header("Player")]
        public AudioClip SwingSFX;
        public AudioClip HitEnemySFX;
        public AudioClip GetHitSFX;
        public AudioClip ShotgunSFX;
        public AudioClip PlantSFX;
        public AudioClip HarvestSFX;
        public AudioClip StepSFX;
        public AudioClip WateringSFX;
        public AudioClip DigSFX;
        public AudioClip TillingSFX;
        public AudioClip KickRockSFX;

        [Header("UI")]
        public AudioClip InventoryClickSFX;
        public AudioClip HologramHumSFX;
        public AudioClip BuyItemSFX;
        public AudioClip SellItemSFX;
        public AudioClip HologramActivateSFX;
        public AudioClip HologramDeactivateSFX;

        [Header("Enemy")]
        public AudioClip EnemyGetHitSFX;

        public AudioClip NibletAttackSFX;
        public AudioClip SkybiteAttackSFX;
        public AudioClip GnasherAttackSFX;
        public AudioClip GluttonAttackSFX;

        [Header("Plants")]
        public AudioClip BloomburstShotSFX;
        public AudioClip PitSnapperMunchSFX;
        public AudioClip SpikeBrambleHitSFX;


        void Awake()
        {
            if (_sfxManager == null)
            {
                _sfxManager = this;
            }

            _audioSource = GetComponent<AudioSource>();
        }

        public void PlaySound(string sound)
        {
            switch (sound.ToLower())
            {
                // Player SFX
                case "swing":
                    _audioSource.PlayOneShot(SwingSFX);
                    break;
                case "hitenemy":
                    _audioSource.PlayOneShot(HitEnemySFX);
                    break;
                case "gethit":
                    _audioSource.PlayOneShot(GetHitSFX);
                        break;
                case "shotgun":
                    _audioSource.PlayOneShot(ShotgunSFX);
                    break;
                case "plant":
                    _audioSource.PlayOneShot(PlantSFX);
                    break;
                case "harvest":
                    _audioSource.PlayOneShot(HarvestSFX);
                        break;
                case "step":
                    _audioSource.PlayOneShot(StepSFX);
                    break;
                case "watering":
                    _audioSource.PlayOneShot(WateringSFX);
                    break;
                case "dig":
                    _audioSource.PlayOneShot(DigSFX);
                    break;
                case "tilling":
                    _audioSource.PlayOneShot(TillingSFX);
                    break;
                case "kickrock":
                    _audioSource.PlayOneShot(KickRockSFX);
                    break;

                // UI SFX
                case "inventoryclick":
                    _audioSource.PlayOneShot(InventoryClickSFX);
                    break;
                case "hologramhum":
                    _audioSource.PlayOneShot(HologramHumSFX);
                    break;
                case "buyitem":
                    _audioSource.PlayOneShot(BuyItemSFX);
                    break;
                case "sellitem":
                    _audioSource.PlayOneShot(SellItemSFX);
                    break;
                case "hologramactivate":
                    _audioSource.PlayOneShot(HologramActivateSFX);
                    break;
                case "hologramdeactivate":
                    _audioSource.PlayOneShot(HologramDeactivateSFX);
                    break;

                // Enemy SFX
                case "enemygethit":
                    _audioSource.PlayOneShot(EnemyGetHitSFX);
                    break;
                case "nibletattack":
                    _audioSource.PlayOneShot(NibletAttackSFX);
                    break;
                case "skybiteattack":
                    _audioSource.PlayOneShot(SkybiteAttackSFX);
                    break;
                case "gnasherattack":
                    _audioSource.PlayOneShot(GnasherAttackSFX);
                    break;
                case "gluttonattack":
                    _audioSource.PlayOneShot(GluttonAttackSFX);
                    break;

                //Plant SFX
                case "bloomburstshot":
                    _audioSource.PlayOneShot(BloomburstShotSFX);
                    break;
                case "pitsnappermunch":
                    _audioSource.PlayOneShot(PitSnapperMunchSFX);
                    break;
                case "spikebramblehit":
                    _audioSource.PlayOneShot(SpikeBrambleHitSFX);
                    break;

                default:
                    _audioSource.PlayOneShot(SwingSFX); // default to swing
                    break;
            }
        }

        // Static wrappers so other scripts can call SFXManager without keeping a reference
        public static void Play(string sound)
        {
            _sfxManager?.PlaySound(sound);
        }

        public static void PlayClip(AudioClip clip)
        {
            if (_sfxManager == null || clip == null) return;
            _sfxManager._audioSource.PlayOneShot(clip);
        }
    }
}
