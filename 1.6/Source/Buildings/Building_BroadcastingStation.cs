using Verse;
using RimWorld;
using VEF.Buildings;
using RimWorld.Planet;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
    public class Building_BroadcastingStation : StudiableBuilding
    {
        public override void Study(Pawn pawn)
        {
            if (Map.Parent is Site site)
            {
                QuestUtility.SendQuestTargetSignals(site.questTags, "VQE_BroadcastingStationIntercepted", site.Named("SUBJECT"));
            }
            else if (Map.Parent is PocketMapParent pocketMapParent && pocketMapParent.sourceMap.Parent is Site site1)
            {
                QuestUtility.SendQuestTargetSignals(site1.questTags, "VQE_BroadcastingStationIntercepted", site1.Named("SUBJECT"));
            }
            base.Study(pawn);
        }
    }
}
