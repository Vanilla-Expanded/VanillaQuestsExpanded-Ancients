using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System;
using RimWorld.Planet;

namespace VanillaQuestsExpandedAncients
{
    public class QuestNode_Root_AncientResearchVault : QuestNode_Site
    {
        public override SitePartDef QuestSite => InternalDefOf.VQEA_AncientResearchVaultSite;
        public override Predicate<Map, PlanetTile> TileValidator => (Map map, PlanetTile tile)
            => map == null || (Find.WorldGrid.ApproxDistanceInTiles(tile, map.Tile) >= 15 && Find.WorldGrid.ApproxDistanceInTiles(tile, map.Tile) <= 45);
        protected override void RunInt()
        {
            if (PrepareQuest(out var map, out var points, out var tile))
            {
                var site = GenerateSite(points, tile, Faction.OfAncientsHostile, out var siteMapGeneratedSignal, out _);
                if (site != null)
                {
                    var questPart = new QuestPart_AncientLab
                    {
                        mapParent = site,
                        inSignalSuccess = QuestGenUtility.HardcodedSignalWithQuestID("site.VQE_SealedVaultDoorUnlocked"),
                        inSignalFail = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved"),
                        questBuilding = InternalDefOf.VQEA_LockedVaultDoor,
                    };
                    QuestGen.quest.AddPart(questPart);
                }
            }
        }
    }
}
