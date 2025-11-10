using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System;
using RimWorld.Planet;

namespace VanillaQuestsExpandedAncients
{
    public class QuestNode_Root_ArchiteArraySite : QuestNode_Site
    {
        public override SitePartDef QuestSite => InternalDefOf.VQEA_ArchiteArraySite;
        public override Predicate<Map, PlanetTile> TileValidator => (Map map, PlanetTile tile)
            => map == null || (Find.WorldGrid.ApproxDistanceInTiles(tile, map.Tile) >= 30 && Find.WorldGrid.ApproxDistanceInTiles(tile, map.Tile) <= 45);
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
                        site = site,
                        inSignalSuccess = QuestGenUtility.HardcodedSignalWithQuestID("site.VQE_ArrayUninstalled"),
                        inSignalFail = QuestGenUtility.HardcodedSignalWithQuestID("site.VQE_ArrayDestroyed")
                    };
                    QuestGen.quest.AddPart(questPart);
                }
            }
        }
    }
}
