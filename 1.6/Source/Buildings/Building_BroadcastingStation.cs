using Verse;
using RimWorld;
using VEF.Buildings;


namespace VanillaQuestsExpandedAncients
{
    public class Building_BroadcastingStation : StudiableBuilding
    {
        public override void Study(Pawn pawn)
        {
            base.Study(pawn);

            //Quest triggering code

            /*QuestNode_Root_AncientSilo.noAsker = true;
            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(InternalDefOf.VQE_Deadlife_AncientSilo, StorytellerUtility.DefaultThreatPointsNow(Find.World));
            QuestUtility.SendLetterQuestAvailable(quest);
            QuestNode_Root_AncientSilo.noAsker = false;*/
        }
    }
}
