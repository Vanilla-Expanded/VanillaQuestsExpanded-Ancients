using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    public class WorkGiver_EnterArchogenInjector : WorkGiver_EnterBuilding
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(InternalDefOf.VQEA_ArchogenInjector);

    }
}