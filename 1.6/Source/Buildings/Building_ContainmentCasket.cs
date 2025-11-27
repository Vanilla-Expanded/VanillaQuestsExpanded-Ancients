using RimWorld;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class Building_ContainmentCasket : Building_AncientCryptosleepPod
    {
        public bool isStartingScenarioBuidling;
        public ContainmentDetails Props => def.GetModExtension<ContainmentDetails>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isStartingScenarioBuidling, "shouldBePlayerFaction");
        }
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

        public override void EjectContents()
        {
            var map = Map;
            if (this.ContainedThing is Pawn pawn && isStartingScenarioBuidling && pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
            }
            base.EjectContents();
            allowDestroyNonDestroyable = true;
            this.Destroy();
            allowDestroyNonDestroyable = false;
            var pod = ThingMaker.MakeThing(InternalDefOf.VQEA_AncientLaboratoryCasket_Empty);
            GenSpawn.Spawn(pod, Position, map, Rotation);
        }

       



    }
}
