using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class CompProperties_SignalOnDestroy : CompProperties
    {
        public string signal;

        public CompProperties_SignalOnDestroy()
        {
            compClass = typeof(CompSignalOnDestroy);
        }
    }
    public class CompSignalOnDestroy : ThingComp
    {
        public CompProperties_SignalOnDestroy Props => (CompProperties_SignalOnDestroy)props;

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (previousMap?.Parent is Site site)
            {
                QuestUtility.SendQuestTargetSignals(site.questTags, Props.signal, site.Named("SUBJECT"));
            }
            else if (previousMap?.Parent is PocketMapParent pocketMapParent && pocketMapParent.sourceMap.Parent is Site site1 )
            {
                QuestUtility.SendQuestTargetSignals(site1.questTags, Props.signal, site1.Named("SUBJECT"));
            }
        }
    }
}
