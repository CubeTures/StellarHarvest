using UnityEngine;

namespace Sunspore
{
    public class HomingBullet : MonoBehaviour
    {
        private Transform _target;
        private Rigidbody _rb;
        private Collider _collider;
        private AudioSource _audioSource;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private int _damage = 15;
        [SerializeField] private float _lifetime = 5f;
        private bool _hit = false;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _missSound;
        [SerializeField] private GameObject _hitEffect;
        [SerializeField] private GameObject _missEffect;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_target != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, _target.position, _speed * Time.fixedDeltaTime);
            }
            _lifetime -= Time.fixedDeltaTime;
            if (_lifetime <= 0)
            {
                PlaySound(_missSound);
                Instantiate(_missEffect, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                if (!_hit)
                {
                    _hit = true;
                    PlaySound(_hitSound);
                    Instantiate(_hitEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                    other.GetComponent<Enemy>()?.TakeDamage(_damage, Vector2.zero, 0f);
                }
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
