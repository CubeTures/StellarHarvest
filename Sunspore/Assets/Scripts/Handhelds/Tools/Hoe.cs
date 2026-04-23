using UnityEngine;

namespace Sunspore
{
    public class Hoe : HandHeld
    {
        private const int NonClickableLayer = 1; // "Non Clickable"

        public override bool TryGetAbilityPreview(Player player, Ray ray, out Vector3 previewCenter, out Vector3 previewSize, out bool canUse)
        {
            previewCenter = default;
            previewSize = default;
            canUse = false;

            int mask = LayerMask.GetMask("Ground") & ~(1 << NonClickableLayer);
            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, mask))
            {
                return false;
            }

            if (TerrainTilemap.Instance == null)
            {
                return false;
            }

            previewCenter = TerrainTilemap.Instance.GetCellCenterWorld(hit.point);
            previewSize = TerrainTilemap.Instance.GetCellWorldSize();
            canUse = TerrainTilemap.Instance.CheckRange(hit.point, abilityRange, player.transform.position);
            return true;
        }

        public override void Ability(Player player, Ray ray)
        {
            int mask = LayerMask.GetMask("Ground") & ~(1 << NonClickableLayer);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, mask))
            {
                TerrainTilemap.Instance.TillAtWorldPosition(hit.point, abilityRange, player.transform.position);
                // Tilling loop handled by HandHeldManager via AbilityLoopSfxKey
                SFXManager.Play("plant");
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
        
            public override string AbilityLoopSfxKey()
            {
                return "tilling";
            }
    }
}
