using UnityEditor.XR;
using UnityEngine;

namespace Sunspore
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] int health = 100;
        [SerializeField] int damage = 10;
        [SerializeField] Player player;

        public void TakeDamage(int weaponDamage, Vector2 direction, float knockback)
        {
            health -= weaponDamage;
            if (health <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
