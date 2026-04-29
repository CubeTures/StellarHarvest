using UnityEngine;

namespace Sunspore
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] GameObject crateExplosionPrefab;
        [SerializeField] AudioClip crateExplosionSound;
        [SerializeField] ItemTypeAndAmount[] items;

        AudioSource _audioSource;

        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Instantiate(crateExplosionPrefab, transform.position, Quaternion.identity);
                PlaySound(crateExplosionSound);
                SFXManager.Play("shotgun");

                foreach (var entry in items)
                {
                    InventoryManager.Instance.AddItem(entry.item, entry.amount);
                }

                Destroy(gameObject);
            }

            if (collision.gameObject.CompareTag("Ground"))
            {
                // Debug.Log("Crate hit ground");
                SFXManager.Play("gethit");
            }
        }

        void PlaySound(AudioClip sound)
        {
            if (sound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(sound);
            }
        }
    }
}
