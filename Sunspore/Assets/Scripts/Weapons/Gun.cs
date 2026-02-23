using UnityEngine;

namespace Sunspore
{
    public class Gun : Weapon
    {
        [SerializeField] float speed = 25f;
        [SerializeField] private GameObject bulletPrefab;

        public override void Attack(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mouseDir, Vector3 playerPos)
        {
            //Debug.Log("Attack");
            GameObject bullet = Instantiate(bulletPrefab, playerPos, Quaternion.identity);
            bullet.GetComponent<WeaponBullet>().Shoot(attackDuration, damage, speed, mouseDir);
        }
    }
}
