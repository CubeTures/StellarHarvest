using UnityEngine;

namespace Sunspore
{
    public class Shovel : HandHeld
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
                    Destroy(plant.gameObject);
                    // tool till/dig loop handled by HandHeldManager via AbilityLoopSfxKey
                    //Debug.Log("Plant Removed");
                //else
                //{
                //    Debug.Log("Plant is out of range!");
                //}
                }
            }
        }
        
            public override string AbilityLoopSfxKey()
            {
                return "dig";
            }
    }
}
