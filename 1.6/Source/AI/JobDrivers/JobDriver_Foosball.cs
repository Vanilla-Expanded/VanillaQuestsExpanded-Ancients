
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    public class JobDriver_Foosball : JobDriver_SitFacingBuilding
    {
       

        protected override void ModifyPlayToil(Toil toil)
        {
          
            base.ModifyPlayToil(toil);
            toil.PlaySustainerOrSound(() => InternalDefOf.VQEA_Foosball_Ambience, 1);
            toil.handlingFacing = true;
            toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
           
        }
    }
}
