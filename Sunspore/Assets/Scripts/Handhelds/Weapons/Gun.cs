using UnityEngine;

namespace Sunspore
{
    public class Gun : HandHeld
    {
        [SerializeField] float speed = 25f;
        [SerializeField] private GameObject bulletPrefab;

        public override void Attack(Vector3 boxCenter, Vector3 hitArea, Quaternion boxRotation, Vector3 mouseDir, Vector3 playerPos)
        {
            //Debug.Log("Attack");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float playerY = playerPos.y;
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, playerY, 0));

            float enter;
            Vector3 intersection = new Vector3();
            if (groundPlane.Raycast(ray, out enter))
            {
                intersection = ray.GetPoint(enter);
                // intersection is the point where the ray hits the plane at y = playerY
            }

            Vector3 dir = (intersection - playerPos).normalized;

            GameObject bullet = Instantiate(bulletPrefab, playerPos, Quaternion.identity);
            bullet.GetComponent<WeaponBullet>().Shoot(attackDuration, damage, speed, dir);
        }
    }
}
