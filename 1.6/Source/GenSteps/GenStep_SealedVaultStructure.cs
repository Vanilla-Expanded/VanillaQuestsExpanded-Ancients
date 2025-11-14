using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using RimWorld.Planet;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
    public class GenStep_SealedVaultStructure : GenStep
    {
        public ThingDef exitDef;
        public override int SeedPart => 987654322;
        public override void Generate(Map map, GenStepParams parms)
        {
            var scenpart = Find.Scenario.AllParts.OfType<ScenPart_SealedVault>().FirstOrDefault();
            if (scenpart != null && scenpart.structureSetDef != null && scenpart.mapParent == (map.Parent as PocketMapParent).sourceMap.Parent)
            {
                StructureSetGenerator.Generate(map, scenpart.structureSetDef, map.ParentFaction);
                var cond = GameConditionMaker.MakeConditionPermanent(InternalDefOf.VQEA_AncientComplex);
                map.gameConditionManager.RegisterCondition(cond);

                var thing = map.listerThings.ThingsOfDef(exitDef).First();
                MapGenerator.PlayerStartSpot = thing.Position;
                scenpart.structureSetDef = null;
            }
        }
    }
}
