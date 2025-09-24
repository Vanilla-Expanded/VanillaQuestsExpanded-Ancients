using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    public class WorkGiver_CarryToArchogenInjector : WorkGiver_CarryToBuilding
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(InternalDefOf.VQEA_ArchogenInjector);

    }
}