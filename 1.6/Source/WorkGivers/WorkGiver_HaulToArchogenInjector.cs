using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    public class WorkGiver_HaulToArchogenInjector : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(InternalDefOf.VQEA_ArchogenInjector);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !ModsConfig.BiotechActive;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_ArchogenInjector building_ArchogenInjector))
            {
                return false;
            }
            if (building_ArchogenInjector.State != ArchiteInjectorState.WaitingForCapsule)
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            return FindIngredients(pawn, building_ArchogenInjector).Thing != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_ArchogenInjector building_ArchogenInjector))
            {
                return null;
            }
            if (building_ArchogenInjector.State != ArchiteInjectorState.WaitingForCapsule)
            {
                return null;
            }
            ThingCount thingCount = FindIngredients(pawn, building_ArchogenInjector);
            if (thingCount.Thing != null)
            {
                Job job = HaulAIUtility.HaulToContainerJob(pawn, thingCount.Thing, t);
                job.count = Mathf.Min(job.count, thingCount.Count);
                return job;
            }
            return null;
        }

        private ThingCount FindIngredients(Pawn pawn, Building_ArchogenInjector injector)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.ArchiteCapsule), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 99f, Validator);
            if (thing == null)
            {
                return default(ThingCount);
            }
            int requiredCountOf = injector.GetRequiredCountOf(thing.def);
            return new ThingCount(thing, Mathf.Min(thing.stackCount, requiredCountOf));
            bool Validator(Thing x)
            {
                if (!pawn.CanReserve(x))
                {
                    return false;
                }
                if (x.IsForbidden(pawn))
                {
                    return false;
                }
                return injector.CanAcceptIngredient(x);
            }
        }
    }
}
