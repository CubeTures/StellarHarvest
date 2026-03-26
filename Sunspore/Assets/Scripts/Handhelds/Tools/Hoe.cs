using UnityEngine;

namespace Sunspore
{
    public class Hoe : HandHeld
    {
        public override void Ability(Player player, Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TerrainTilemap.Instance.TillAtWorldPosition(hit.point, abilityRange, player.transform.position);
            }
        }

        public override bool CheckRange(Player player, Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return TerrainTilemap.Instance.CheckRange(hit.point, abilityRange, player.transform.position);
            }

            return false;
        }
    }
}
