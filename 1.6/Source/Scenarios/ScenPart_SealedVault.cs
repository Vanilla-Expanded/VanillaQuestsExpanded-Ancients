using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class ScenPart_SealedVault : ScenPart
    {
        public StructureSetDef structureSetDef;
        public MapParent mapParent;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref structureSetDef, "structureSetDef");
            Scribe_References.Look(ref mapParent, "mapParent");
        }

        public override void PostMapGenerate(Map map)
        {
            if (Find.GameInitData != null)
            {
                mapParent = map.Parent;
                StructureSetGenerator.Generate(map, InternalDefOf.VQEA_SealedVaultStartStructure, Faction.OfPlayer);
                ReplaceSarcophagusContents(map, InternalDefOf.VQEA_AncientLaboratoryCasket, InternalDefOf.VQE_Patient);
                ReplaceSarcophagusContents(map, InternalDefOf.VQEA_CandidateCryptosleepCasket, InternalDefOf.VQE_Experiment);
            }
        }

        private void ReplaceSarcophagusContents(Map map, ThingDef sarcophagusDef, PawnKindDef pawnKind)
        {
            var sarcophagi = map.listerThings.ThingsOfDef(sarcophagusDef).Cast<Building_CryptosleepCasket>().ToList();
            foreach (var sarcophagus in sarcophagi)
            {
                if (sarcophagus.HasAnyContents)
                {
                    sarcophagus.GetDirectlyHeldThings().ClearAndDestroyContents();
                }
                var pawn = PawnGenerator.GeneratePawn(pawnKind, Faction.OfPlayer);
                sarcophagus.TryAcceptThing(pawn);
            }
        }
    }
}
