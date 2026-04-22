using UnityEngine;

namespace Sunspore
{
    public class WateringCan : HandHeld
    {
        public override bool TryGetAbilityPreview(Player player, Ray ray, out Vector3 previewCenter, out Vector3 previewSize, out bool canUse)
        {
            previewCenter = default;
            previewSize = default;
            canUse = false;

            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                return false;
            }

            Plant plant = hit.collider.GetComponentInParent<Plant>();
            if (plant == null)
            {
                return false;
            }

            previewCenter = plant.transform.position;
            previewSize = GetPlantBoundsSize(plant);
            canUse = Vector3.Distance(player.transform.position, plant.transform.position) <= abilityRange;
            return true;
        }

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

        private static Vector3 GetPlantBoundsSize(Plant plant)
        {
            if (plant == null)
            {
                return new Vector3(0.8f, 0f, 0.8f);
            }

            Collider[] colliders = plant.GetComponentsInChildren<Collider>();
            if (colliders == null || colliders.Length == 0)
            {
                return new Vector3(0.8f, 0f, 0.8f);
            }

            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return new Vector3(Mathf.Max(0.8f, bounds.size.x), 0f, Mathf.Max(0.8f, bounds.size.z));
        }
    }
}
