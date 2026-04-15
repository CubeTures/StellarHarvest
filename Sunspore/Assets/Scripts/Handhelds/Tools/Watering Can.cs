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
                    plant.WaterPlant();
                    TutorialManager.Instance.BroadcastSignal(TutorialManager.Actions.WATERING_CAN);
                    //Debug.Log("Plant Watered");
                    //else
                    //{
                    //    Debug.Log("Plant is out of range!");
                    //}
                }
            }
        }
    }
}
