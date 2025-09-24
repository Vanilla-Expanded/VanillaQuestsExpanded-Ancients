using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    public class JobGiver_TrashBuildings : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            // Find all colonist buildings that are destructible (structures and walls) but not the archogen injector
            var attackableBuildings = pawn.Map.listerBuildings.allBuildingsColonist
                .Where(building => building.def != InternalDefOf.VQEA_ArchogenInjector && building.MaxHitPoints > 0)
                .OrderBy(b => b.Position.DistanceTo(pawn.Position))
                .ToList();

            if (attackableBuildings.Any())
            {
                var target = attackableBuildings.First();
                return JobMaker.MakeJob(JobDefOf.AttackMelee, target);
            }

            // If no buildings or walls are available, the mutant will look for other targets via other job givers
            // but should not attack colonist pawns unless already in melee combat
            return null;
        }
    }
}
