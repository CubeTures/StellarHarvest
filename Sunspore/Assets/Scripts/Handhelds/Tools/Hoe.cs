using UnityEngine;

namespace Sunspore
{
    public class Hoe : HandHeld
    {
        private const int NonClickableLayer = 1; // "Non Clickable"

        public override void Ability(Player player, Ray ray)
        {
            int mask = LayerMask.GetMask("Ground") & ~(1 << NonClickableLayer);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, mask))
            {
                TerrainTilemap.Instance.TillAtWorldPosition(hit.point, abilityRange, player.transform.position);
                SFXManager._sfxManager.PlaySound("plant");
                TutorialManager.Instance.BroadcastSignal(TutorialManager.Actions.HOE);
            }
        }

        public override bool CheckRange(Player player, Ray ray)
        {
            int mask = LayerMask.GetMask("Ground") & ~(1 << NonClickableLayer);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, mask))
            {
                return TerrainTilemap.Instance.CheckRange(hit.point, abilityRange, player.transform.position);
            }

            return false;
        }
    }
}
