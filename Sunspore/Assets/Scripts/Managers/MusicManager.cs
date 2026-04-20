using System.Collections;
using UnityEngine;

namespace Sunspore
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] AudioClip dayMusic;
        [SerializeField] AudioClip nightMusic;
        [SerializeField] float fadeDuration = 2f;

        AudioSource _audioSource;
        [HideInInspector] public float targetVolume;
        Coroutine _fadeCoroutine;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            targetVolume = _audioSource.volume;
        }

        public void PlayDayMusic()
        {
            CrossfadeTo(dayMusic);
        }

        public void PlayNightMusic()
        {
            CrossfadeTo(nightMusic);
        }

        private void CrossfadeTo(AudioClip newClip)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip));
        }

        private IEnumerator CrossfadeCoroutine(AudioClip newClip)
        {
            if (_audioSource.isPlaying)
            {
                float startVolume = _audioSource.volume;
                for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
                {
                    _audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                    yield return null;
                }
                _audioSource.volume = 0f;
            }
            else
            {
                _audioSource.volume = 0f;
            }

            _audioSource.clip = newClip;
            _audioSource.Play();

            for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
            {
                _audioSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
                yield return null;
            }
            _audioSource.volume = targetVolume;

            _fadeCoroutine = null;
        }
    }
}
