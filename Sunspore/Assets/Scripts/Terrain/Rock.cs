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
                _spriteRenderer.sortingOrder = (int)(-transform.position.z * 100);
            }
        }
    }
}
