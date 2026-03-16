using UnityEngine;

namespace Sunspore
{
    public class WateringCan : HandHeld
    {
        public override void Ability(Player player, Ray ray)
        {
            //Debug.Log(ray);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log("hit");
                Plant plant = hit.collider.GetComponent<Plant>();
                if (plant != null)
                {
                    float distance = Vector3.Distance(player.transform.position, plant.transform.position);
                    if (distance <= abilityRange)
                    {
                        plant.WaterPlant();
                        //Debug.Log("Plant Watered");
                    }
                    //else
                    //{
                    //    Debug.Log("Plant is out of range!");
                    //}
                }
            }
        }
    }
}
