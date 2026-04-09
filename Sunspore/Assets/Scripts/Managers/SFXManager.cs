using UnityEngine;

namespace Sunspore
{
    public class SFXManager : MonoBehaviour
    {
        public static SFXManager _sfxManager;
        AudioSource _audioSource;

        [Header("SFX")]
        public AudioClip SwingSFX;
        public AudioClip ShotgunSFX;
        public AudioClip PlantSFX;

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
