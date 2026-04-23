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
//      public AudioClip HitEnemySFX;
        public AudioClip GetHitSFX;
        public AudioClip ShotgunSFX;
        public AudioClip PlantSFX;
        public AudioClip HarvestSFX;
        public AudioClip StepSFX;
        public AudioClip WateringSFX;
        public AudioClip DigSFX;
        //public AudioClip TillingSFX;
        public AudioClip KickRockSFX;

        [Header("UI")]
        public AudioClip InventoryClickSFX;
        public AudioClip HologramHumSFX;
        public AudioClip BuyItemSFX;
        public AudioClip SellItemSFX;
        public AudioClip HologramActivateSFX;
        public AudioClip HologramDeactivateSFX;

        [Header("Enemy")]
        public AudioClip NibletGetHitSFX;
        public AudioClip SkybiteGetHitSFX;
        public AudioClip GnasherGetHitSFX;
        public AudioClip GluttonGetHitSFX;
        

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
            var clip = GetClipByName(sound);
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip);
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

        // Play an AudioClip on the manager's AudioSource and loop it until stopped.
        public void PlayLoopClip(AudioClip clip)
        {
            if (clip == null || _audioSource == null) return;

            // If already looping the same clip, do nothing
            if (_audioSource.isPlaying && _audioSource.loop && _audioSource.clip == clip) return;

            _audioSource.clip = clip;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        // Stop any looping playback started via PlayLoopClip
        public void StopLooping()
        {
            if (_audioSource == null) return;
            _audioSource.loop = false;
            _audioSource.Stop();
            _audioSource.clip = null;
        }

        // Static wrappers so other scripts can start/stop looping without a direct reference
        public static void PlayLoop(AudioClip clip)
        {
            if (_sfxManager == null || clip == null) return;
            _sfxManager.PlayLoopClip(clip);
        }

        // Instance method to play a named sound in a loop (uses same keys as PlaySound)
        public void PlayLoopByName(string sound)
        {
            var clip = GetClipByName(sound);
            if (clip != null)
            {
                PlayLoopClip(clip);
            }
        }

        // Helper to map a string key to the appropriate AudioClip
        AudioClip GetClipByName(string sound)
        {
            if (string.IsNullOrEmpty(sound)) return SwingSFX;

            switch (sound.ToLower())
            {
                // Player SFX
                case "swing": return SwingSFX;
                case "gethit": return GetHitSFX;
                case "shotgun": return ShotgunSFX;
                case "plant": return PlantSFX;
                case "harvest": return HarvestSFX;
                case "step": return StepSFX;
                case "watering": return WateringSFX;
                case "dig": return DigSFX;
                case "kickrock": return KickRockSFX;

                // UI SFX
                case "inventoryclick": return InventoryClickSFX;
                case "hologramhum": return HologramHumSFX;
                case "buyitem": return BuyItemSFX;
                case "sellitem": return SellItemSFX;
                case "hologramactivate": return HologramActivateSFX;
                case "hologramdeactivate": return HologramDeactivateSFX;

                // Enemy SFX
                case "nibletgethit": return NibletGetHitSFX;
                case "skybitegethit": return SkybiteGetHitSFX;
                case "gnashergethit": return GnasherGetHitSFX;
                case "gluttongethit": return GluttonGetHitSFX;
                case "nibletattack": return NibletAttackSFX;
                case "skybiteattack": return SkybiteAttackSFX;
                case "gnasherattack": return GnasherAttackSFX;
                case "gluttonattack": return GluttonAttackSFX;

                // Plant SFX
                case "bloomburstshot": return BloomburstShotSFX;
                case "pitsnappermunch": return PitSnapperMunchSFX;
                case "spikebramblehit": return SpikeBrambleHitSFX;

                default: return SwingSFX;
            }
        }

        // Static wrapper for name-based looping
        public static void PlayLoop(string sound)
        {
            if (_sfxManager == null || string.IsNullOrEmpty(sound)) return;
            _sfxManager.PlayLoopByName(sound);
        }

        public static void StopLoop()
        {
            _sfxManager?.StopLooping();
        }
    }
}
