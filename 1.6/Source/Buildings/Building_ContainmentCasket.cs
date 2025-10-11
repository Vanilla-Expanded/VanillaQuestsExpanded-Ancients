using RimWorld;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class Building_ContainmentCasket : Building_AncientCryptosleepPod
    {
        public ContainmentDetails Props => def.GetModExtension<ContainmentDetails>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
                CreateContainedPawn();
            }
        }

        protected void CreateContainedPawn()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(
                kind: Props.containmentSpawn,
                faction: null,
                tile: this.Map.Tile,
                forceGenerateNewPawn: true,
                canGeneratePawnRelations: false,
                forceAddFreeWarmLayerIfNeeded: false,
                allowFood: false,
                allowAddictions: false,
                inhabitant: false,
                certainlyBeenInCryptosleep: true
            );

            Pawn pawn = PawnGenerator.GeneratePawn(request);
            this.GetDirectlyHeldThings().TryAdd(pawn);
        }
    }
}
