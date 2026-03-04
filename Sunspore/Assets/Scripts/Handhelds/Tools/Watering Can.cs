using UnityEngine;

namespace Sunspore
{
    public class WateringCan : HandHeld
    {
        public override void Ability(Vector3 playerPos)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.Log(ray);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log("hit");
                Plant plant = hit.collider.GetComponent<Plant>();
                if (plant != null)
                {
                    float distance = Vector3.Distance(playerPos, plant.transform.position);
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
