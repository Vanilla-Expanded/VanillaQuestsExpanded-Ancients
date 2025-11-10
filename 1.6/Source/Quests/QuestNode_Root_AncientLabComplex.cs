using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System;
using RimWorld.Planet;

namespace VanillaQuestsExpandedAncients
{
    public class QuestNode_Root_AncientLabComplex : QuestNode_Site
    {
        public override SitePartDef QuestSite => InternalDefOf.VQEA_AncientLabComplexSite;
        public override Predicate<Map, PlanetTile> TileValidator => (Map map, PlanetTile tile)
            => map == null || (Find.WorldGrid.ApproxDistanceInTiles(tile, map.Tile) >= 15 && Find.WorldGrid.ApproxDistanceInTiles(tile, map.Tile) <= 30);
        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            slate.Set("askerIsNull", true);
            if (PrepareQuest(out var map, out var points, out var tile))
            {
                var site = GenerateSite(points, tile, Faction.OfAncientsHostile, out var siteMapGeneratedSignal, out _);
                if (site != null)
                {
                    var questPart = new QuestPart_AncientLab
                    {
                        mapParent = site,
                        inSignalSuccess = QuestGenUtility.HardcodedSignalWithQuestID("site.VQE_BroadcastingStationIntercepted"),
                        inSignalFail = QuestGenUtility.HardcodedSignalWithQuestID("site.VQE_BroadcastingStationDestroyed")
                    };
                    QuestGen.quest.AddPart(questPart);
                }
            }
        }
    }
}
