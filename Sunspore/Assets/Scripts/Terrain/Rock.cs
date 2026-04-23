using UnityEngine;

namespace Sunspore
{
    public class Rock : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _lastPosition = transform.position;
            UpdateSortingOrder();
        }

        void Update()
        {
            if (transform.position != _lastPosition)
            {
                _lastPosition = transform.position;
                UpdateSortingOrder();

                if (transform.position.y < 0f)
                {
                    transform.position = new Vector3(
                        transform.position.x,
                        0f,
                        transform.position.z
                    );
                }
            }
        }

        private void UpdateSortingOrder()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = (int)(-transform.position.z * 100) - 50 * 100;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Play kick rock SFX when the player collides with sufficient force
            if (collision.gameObject != null && collision.gameObject.CompareTag("Player"))
            {
                float impact = collision.relativeVelocity.magnitude;
                if (impact > 0.5f)
                {
                    SFXManager.Play("kickrock");
                }
            }
        }
    }
}
