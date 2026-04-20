using UnityEngine;

namespace Sunspore
{
    public class Grass : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sortingOrder = (int)(-transform.position.z * 100) - 50 * 100;
        }

        public void DestroyGrass()
        {
            Destroy(gameObject);
        }
    }
}
