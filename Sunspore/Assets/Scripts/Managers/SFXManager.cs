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
                case "swing":
                    _audioSource.PlayOneShot(SwingSFX);
                    break;
                case "shotgun":
                    _audioSource.PlayOneShot(ShotgunSFX);
                    break;
                case "plant":
                    _audioSource.PlayOneShot(PlantSFX);
                    break;
                default:
                    _audioSource.PlayOneShot(SwingSFX); // default to swing
                    break;
            }
        }
    }
}
