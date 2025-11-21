using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    public class WorkGiver_CarryToAncientWonderdoc : WorkGiver_CarryToBuilding
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(InternalDefOf.VQEA_AncientWonderdoc);
    }
}