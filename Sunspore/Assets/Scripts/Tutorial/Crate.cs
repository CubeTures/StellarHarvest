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

                foreach (var entry in items)
                {
                    InventoryManager.Instance.AddItem(entry.item, entry.amount);
                }

                Destroy(gameObject);
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
