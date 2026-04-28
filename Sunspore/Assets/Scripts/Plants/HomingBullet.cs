using UnityEngine;

namespace Sunspore
{
    public class HomingBullet : MonoBehaviour
    {
        private Transform _target;
        private Rigidbody _rb;
        private Collider _collider;
        private AudioSource _audioSource;
        private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private int _damage = 15;
        [SerializeField] private float _lifetime = 5f;
        private bool _hit = false;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _missSound;
        [SerializeField] private GameObject _hitEffect;
        [SerializeField] private GameObject _missEffect;

        [Header("Retargeting")]
        [SerializeField] private float _retargetRange = 10f;
        [SerializeField] private float _retargetAngle = 45f;

        private Vector3 _direction;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void FixedUpdate()
        {
            if (_target != null)
            {
                Vector3 toTarget = _target.position - transform.position;
                if (toTarget.sqrMagnitude > 0.001f)
                    _direction = toTarget.normalized;

                transform.position = Vector3.MoveTowards(transform.position, _target.position, _speed * Time.fixedDeltaTime);
                UpdateSortingOrder();
            }
            else
            {
                if (TryRetarget())
                {
                    // acquired new target, next frame will home in
                }
                else
                {
                    transform.position += _direction * _speed * Time.fixedDeltaTime;
                    UpdateSortingOrder();
                }
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
            if (_target != null)
            {
                Vector3 toTarget = _target.position - transform.position;
                if (toTarget.sqrMagnitude > 0.001f)
                    _direction = toTarget.normalized;
            }
        }

        private bool TryRetarget()
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, _retargetRange);
            Transform best = null;
            float bestDist = float.MaxValue;

            foreach (Collider col in nearby)
            {
                if (!col.CompareTag("Enemy")) continue;

                Vector3 toCandidate = col.transform.position - transform.position;
                if (toCandidate.sqrMagnitude < 0.001f) continue;

                float angle = Vector3.Angle(_direction, toCandidate);
                if (angle > _retargetAngle) continue;

                float dist = toCandidate.sqrMagnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = col.transform;
                }
            }

            if (best != null)
            {
                _target = best;
                return true;
            }
            return false;
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
                    var e = other.GetComponent<Enemy>();
                    if (e != null)
                    {
                        e.TakeDamage(_damage, Vector2.zero, 0f);
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
