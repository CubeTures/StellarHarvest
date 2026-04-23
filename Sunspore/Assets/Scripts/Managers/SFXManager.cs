using System.Collections.Generic;
using UnityEngine;

/*
 SFXManager - central sound effect manager for Sunspore

 Public API (static helpers):
 - Play(string sound): plays a one-shot sound by name.
 - PlayClip(AudioClip clip): plays a one-shot AudioClip.
 - PlayLoop(string sound): starts a looping instance of the named sound (multiple different loops allowed).
 - PlayLoop(AudioClip clip): starts a looping instance for the provided clip.
 - StopLoop(string sound): stops the specific named loop (does not affect other loops).
 - StopAllLoops(): stops and destroys all active looping instances.
 - StopLoop(): no-op retained for compatibility (use StopLoop(string) to stop a specific loop).

 Key behaviors:
 - A runtime dictionary (_clipMap) maps lowercase string keys to AudioClips for fast lookup.
 - Looping sounds are hosted in per-loop AudioSource components parented to the manager, allowing multiple concurrent loops (e.g. "watering" and "hologramhum").
 - One-shot playback uses the manager's primary AudioSource and PlayOneShot to allow overlap.

 Internal helpers:
 - BuildClipMap(): populates the runtime dictionary from the serialized AudioClip fields.
 - GetClipByName(string): returns the AudioClip for a given key (uses _clipMap).
 - PlayLoopClip(AudioClip): creates/uses a dedicated AudioSource to loop a clip.
 - StopLooping(string): internal stop-by-name implementation.

 Usage examples:
    - SFXManager.Play("watering");
    - SFXManager.PlayLoop("watering");
    - SFXManager.StopLoop("watering");
    - SFXManager.StopAllLoops();
*/

namespace Sunspore
{
    public class SFXManager : MonoBehaviour
    {
        public static SFXManager _sfxManager;
        AudioSource _audioSource;
        // Active looping audio sources keyed by sound name
        Dictionary<string, AudioSource> _loopingSources = new Dictionary<string, AudioSource>();
        // Mapping from string keys to AudioClips for fast lookup
        Dictionary<string, AudioClip> _clipMap = new Dictionary<string, AudioClip>();
        // Per-sound volume mapping (key -> volume 0-1)
        Dictionary<string, float> _volumeMap = new Dictionary<string, float>();
        // Reverse lookup from AudioClip to normalized key (for PlayClip overloads)
        Dictionary<AudioClip, string> _clipToKeyMap = new Dictionary<AudioClip, string>();
        // Recent play timestamps per sound key (used to attenuate overlapping one-shots)
        Dictionary<string, List<float>> _recentPlayTimestamps = new Dictionary<string, List<float>>();
        // Time window (seconds) to consider plays "simultaneous" for attenuation
        public float PolyphonyWindow = 0.5f;

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
        public AudioClip CutGrassSFX;

        [Header("UI")]
        public AudioClip InventoryClickSFX;
        public AudioClip InventoryOpenCloseSFX;
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

        [Header("Volumes")]
        public float SwingVolume = 1f;
        public float HitEnemyVolume = 1f;
        public float GetHitVolume = 1f;
        public float ShotgunVolume = 1f;
        public float PlantVolume = 1f;
        public float HarvestVolume = 1f;
        public float StepVolume = 1f;
        public float WateringVolume = 1f;
        public float DigVolume = 1f;
        public float TillingVolume = 1f;
        public float KickRockVolume = 1f;
        public float CutGrassVolume = 1f;

        public float InventoryClickVolume = 1f;
        public float InventoryOpenCloseVolume = 1f;
        public float HologramHumVolume = 1f;
        public float BuyItemVolume = 1f;
        public float SellItemVolume = 1f;
        public float HologramActivateVolume = 1f;
        public float HologramDeactivateVolume = 1f;

        public float NibletGetHitVolume = 1f;
        public float SkybiteGetHitVolume = 1f;
        public float GnasherGetHitVolume = 1f;
        public float GluttonGetHitVolume = 1f;

        public float NibletAttackVolume = 1f;
        public float SkybiteAttackVolume = 1f;
        public float GnasherAttackVolume = 1f;
        public float GluttonAttackVolume = 1f;

        public float BloomburstShotVolume = 1f;
        public float PitSnapperMunchVolume = 1f;
        public float SpikeBrambleHitVolume = 1f;


        void Awake()
        {
            if (_sfxManager == null)
            {
                _sfxManager = this;
            }

            _audioSource = GetComponent<AudioSource>();
            BuildClipMap();
        }

        void BuildClipMap()
        {
            _clipMap = new Dictionary<string, AudioClip>()
            {
                {"swing", SwingSFX},
                {"gethit", GetHitSFX},
                {"shotgun", ShotgunSFX},
                {"plant", PlantSFX},
                {"harvest", HarvestSFX},
                {"step", StepSFX},
                {"watering", WateringSFX},
                {"dig", DigSFX},
                {"tilling", TillingSFX},
                {"kickrock", KickRockSFX},
                {"hitenemy", HitEnemySFX},
                {"cutgrass", CutGrassSFX},

                {"inventoryclick", InventoryClickSFX},
                {"inventoryopenclose", InventoryOpenCloseSFX},
                {"inventoryopenandclose", InventoryOpenCloseSFX},
                {"hologramhum", HologramHumSFX},
                {"buyitem", BuyItemSFX},
                {"sellitem", SellItemSFX},
                {"buyorsell", BuyItemSFX},
                {"hologramactivate", HologramActivateSFX},
                {"hologramdeactivate", HologramDeactivateSFX},

                {"nibletgethit", NibletGetHitSFX},
                {"skybitegethit", SkybiteGetHitSFX},
                {"gnashergethit", GnasherGetHitSFX},
                {"gluttongethit", GluttonGetHitSFX},
                {"nibletattack", NibletAttackSFX},
                {"skybiteattack", SkybiteAttackSFX},
                {"gnasherattack", GnasherAttackSFX},
                {"gluttonattack", GluttonAttackSFX},

                {"bloomburstshot", BloomburstShotSFX},
                {"pitsnappermunch", PitSnapperMunchSFX},
                {"spikebramblehit", SpikeBrambleHitSFX}
            };

            // Build reverse clip -> key map for PlayClip lookups
            _clipToKeyMap = new Dictionary<AudioClip, string>();
            foreach (var kv in _clipMap)
            {
                if (kv.Value != null && !_clipToKeyMap.ContainsKey(kv.Value))
                    _clipToKeyMap[kv.Value] = kv.Key;
            }

            // Initialize per-sound volume map (keys must match BuildClipMap keys)
            _volumeMap = new Dictionary<string, float>()
            {
                {"swing", SwingVolume},
                {"gethit", GetHitVolume},
                {"shotgun", ShotgunVolume},
                {"plant", PlantVolume},
                {"harvest", HarvestVolume},
                {"step", StepVolume},
                {"watering", WateringVolume},
                {"dig", DigVolume},
                {"tilling", TillingVolume},
                {"kickrock", KickRockVolume},
                {"kickrock", KickRockVolume},
                {"hitenemy", HitEnemyVolume},
                {"cutgrass", CutGrassVolume},

                {"inventoryclick", InventoryClickVolume},
                {"inventoryopenclose", InventoryOpenCloseVolume},
                {"inventoryopenandclose", InventoryOpenCloseVolume},
                {"hologramhum", HologramHumVolume},
                {"buyitem", BuyItemVolume},
                {"sellitem", SellItemVolume},
                {"buyorsell", BuyItemVolume},
                {"hologramactivate", HologramActivateVolume},
                {"hologramdeactivate", HologramDeactivateVolume},

                {"nibletgethit", NibletGetHitVolume},
                {"skybitegethit", SkybiteGetHitVolume},
                {"gnashergethit", GnasherGetHitVolume},
                {"gluttongethit", GluttonGetHitVolume},
                {"nibletattack", NibletAttackVolume},
                {"skybiteattack", SkybiteAttackVolume},
                {"gnasherattack", GnasherAttackVolume},
                {"gluttonattack", GluttonAttackVolume},

                {"bloomburstshot", BloomburstShotVolume},
                {"pitsnappermunch", PitSnapperMunchVolume},
                {"spikebramblehit", SpikeBrambleHitVolume}
            };
        }

        public void PlaySound(string sound)
        {
            var clip = GetClipByName(sound);
            if (clip != null && _audioSource != null)
            {
                _audioSource.pitch = Random.Range(0.8f, 1.2f);
                float vol = 1f;
                string key = null;
                if (!string.IsNullOrEmpty(sound))
                {
                    key = sound.ToLower();
                    if (_volumeMap != null && _volumeMap.TryGetValue(key, out var v)) vol = v;
                }

                // Attenuate based on recent plays of the same sound to avoid loud stacking
                float scale = 1f;
                if (!string.IsNullOrEmpty(key))
                {
                    if (!_recentPlayTimestamps.TryGetValue(key, out var list) || list == null)
                    {
                        list = new List<float>();
                        _recentPlayTimestamps[key] = list;
                    }
                    float now = Time.time;
                    list.RemoveAll(t => now - t > PolyphonyWindow);
                    int count = list.Count + 1; // include this play
                    scale = 1f / Mathf.Sqrt(Mathf.Max(1, count));
                    list.Add(now);
                }

                _audioSource.PlayOneShot(clip, vol * scale);
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
            _sfxManager._audioSource.pitch = Random.Range(0.8f, 1.2f);
            float vol = 1f;
            if (_sfxManager._clipToKeyMap != null && _sfxManager._clipToKeyMap.TryGetValue(clip, out var key))
            {
                if (_sfxManager._volumeMap != null && _sfxManager._volumeMap.TryGetValue(key, out var v)) vol = v;
            }
            _sfxManager._audioSource.PlayOneShot(clip, vol);
        }

        // Runtime API to adjust per-sound volumes
        public static void SetVolume(string sound, float volume)
        {
            if (_sfxManager == null || string.IsNullOrEmpty(sound)) return;
            var key = sound.ToLower();
            var vol = Mathf.Clamp01(volume);
            if (_sfxManager._volumeMap == null)
                _sfxManager._volumeMap = new Dictionary<string, float>();
            _sfxManager._volumeMap[key] = vol;
            // If a looping source is active for this sound, update its volume immediately
            if (_sfxManager._loopingSources != null && _sfxManager._loopingSources.TryGetValue(key, out var src) && src != null)
            {
                src.volume = vol;
            }
        }

        public static float GetVolume(string sound)
        {
            if (_sfxManager == null || string.IsNullOrEmpty(sound)) return 1f;
            var key = sound.ToLower();
            if (_sfxManager._volumeMap != null && _sfxManager._volumeMap.TryGetValue(key, out var v)) return v;
            return 1f;
        }

        // Play an AudioClip in a dedicated looping AudioSource (allows multiple concurrent loops)
        public void PlayLoopClip(AudioClip clip)
        {
            if (clip == null) return;
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();

            // prefer a normalized name as key when possible
            var key = !string.IsNullOrEmpty(clip.name) ? clip.name.ToLower() : clip.GetInstanceID().ToString();

            // If already looping this clip, do nothing
            if (_loopingSources.TryGetValue(key, out var existing) && existing != null && existing.isPlaying)
                return;

            // If an AudioSource exists but isn't playing, reuse it
            if (existing != null)
            {
                existing.clip = clip;
                existing.loop = true;
                existing.pitch = Random.Range(0.8f, 1.2f);
                // set per-sound volume if available
                if (_volumeMap != null && _volumeMap.TryGetValue(key, out var lv)) existing.volume = lv;
                existing.Play();
                return;
            }

            // Create a child GameObject to host the looping AudioSource
            var go = new GameObject($"SFXLoop_{key}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.clip = clip;
            src.pitch = Random.Range(0.8f, 1.2f);
            if (_volumeMap != null && _volumeMap.TryGetValue(key, out var initVol)) src.volume = initVol;
            src.Play();
            _loopingSources[key] = src;
        }

        // Stop any looping playback started via PlayLoopClip (stop all loops)
        public void StopLooping()
        {
            foreach (var kv in _loopingSources)
            {
                if (kv.Value == null) continue;
                kv.Value.loop = false;
                kv.Value.Stop();
                Destroy(kv.Value.gameObject);
            }
            _loopingSources.Clear();
        }

        // Stop a specific looping sound by name
        public void StopLooping(string sound)
        {
            if (string.IsNullOrEmpty(sound)) return;
            var key = sound.ToLower();
            if (_loopingSources.TryGetValue(key, out var src) && src != null)
            {
                src.loop = false;
                src.Stop();
                Destroy(src.gameObject);
                _loopingSources.Remove(key);
            }
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
            if (string.IsNullOrEmpty(sound)) return;
            var clip = GetClipByName(sound);
            if (clip != null)
            {
                // Use lowercase key for consistent lookup
                var key = sound.ToLower();
                // Create loop using the clip but store under the lowercase key
                if (_loopingSources.TryGetValue(key, out var existing) && existing != null && existing.isPlaying)
                    return;

                // If there's an existing source for this key but not playing, reuse it
                if (existing != null)
                {
                    existing.clip = clip;
                    existing.loop = true;
                    existing.pitch = Random.Range(0.8f, 1.2f);
                    if (_volumeMap != null && _volumeMap.TryGetValue(key, out var lv2)) existing.volume = lv2;
                    existing.Play();
                    return;
                }

                var go = new GameObject($"SFXLoop_{key}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = true;
                src.clip = clip;
                src.pitch = Random.Range(0.8f, 1.2f);
                if (_volumeMap != null && _volumeMap.TryGetValue(key, out var initVol2)) src.volume = initVol2;
                src.Play();
                _loopingSources[key] = src;
            }
        }

        // Helper to map a string key to the appropriate AudioClip (uses runtime dictionary)
        AudioClip GetClipByName(string sound)
        {
            if (string.IsNullOrEmpty(sound)) return SwingSFX;
            var key = sound.ToLower();
            if (_clipMap != null && _clipMap.TryGetValue(key, out var clip) && clip != null)
                return clip;
            return SwingSFX;
        }

        // Static wrapper for name-based looping
        public static void PlayLoop(string sound)
        {
            if (_sfxManager == null || string.IsNullOrEmpty(sound)) return;
            _sfxManager.PlayLoopByName(sound);
        }

        // Static stop overloads
        // Deprecated: no-arg StopLoop previously stopped all loops.
        // Keep as a no-op to avoid accidentally stopping unrelated loops.
        public static void StopLoop()
        {
            // Intentionally empty. Use StopLoop(string) to stop a specific loop.
        }

        public static void StopLoop(string sound)
        {
            if (_sfxManager == null || string.IsNullOrEmpty(sound)) return;
            _sfxManager.StopLooping(sound);
        }

        // Explicit method to stop all looping sounds
        public static void StopAllLoops()
        {
            _sfxManager?.StopLooping();
        }
    }
}
