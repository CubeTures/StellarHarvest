using System.Collections;
using UnityEngine;

namespace Sunspore
{
    public class WeaponBullet : MonoBehaviour
    {
        private Rigidbody _rb;
        private Collider _collider;
        private AudioSource _audioSource;
        private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private int _damage = 0;
        private Vector3 direction;
        private bool _hit = false;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _missSound;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void FixedUpdate()
        {
            transform.position += direction * _speed * Time.fixedDeltaTime;
            UpdateSortingOrder();
        }

        public void Shoot(float attackDuration, int damage, float speed, Vector3 mouseDir)
        {
            Debug.Log(attackDuration);
            _damage = damage;
            direction = mouseDir;
            _speed = speed;
            StartCoroutine(Missed(attackDuration));
        }

        private IEnumerator Missed(float delay)
        {
            yield return new WaitForSeconds(delay);
            //Debug.Log("miss");
            PlaySound(_missSound);
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                if (!_hit)
                {
                    _hit = true;
                    PlaySound(_hitSound);
                    Destroy(gameObject);
                    var e = other.GetComponent<Enemy>();
                    if (e != null)
                    {
                        e.TakeDamage(_damage, Vector2.zero, 0f, damageFromPlayer: true);
                        SFXManager.Play("hitenemy");
                    }
                }
            }
        }

        private void UpdateSortingOrder()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = (int)(-transform.position.z * 100) + 1;
            }
        }

        private void PlaySound(AudioClip sound)
        {
            if (sound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(sound);
            }
        }
    }
}
