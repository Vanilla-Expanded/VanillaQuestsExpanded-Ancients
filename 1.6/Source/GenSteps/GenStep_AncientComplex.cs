
using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using RimWorld.Planet;
using VEF.Buildings;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
    public class GenStep_AncientComplex : GenStep
    {
        public ThingDef exitDef;
        public override int SeedPart => 987654321;
        public override void Generate(Map map, GenStepParams parms)
        {
            var pocketMapParent = map.Parent as PocketMapParent;
            var isUnderground = pocketMapParent != null;
            var sitePart = isUnderground is false ? (map.Parent as Site).MainSitePartDef : (pocketMapParent.sourceMap.Parent as Site).MainSitePartDef;
            var extension = sitePart.GetModExtension<AncientComplexSiteExtension>();
            var structureSetDef = isUnderground ? extension.undergroundGenerator : extension.surfaceGenerator;
            var structureRects = StructureSetGenerator.Generate(map, structureSetDef, Faction.OfAncientsHostile);
            if (isUnderground)
            {
                foreach (var current in map.AllCells)
                {
                    var isInsideStructure = false;
                    foreach (var rect in structureRects)
                    {
                        if (rect.Contains(current))
                        {
                            isInsideStructure = true;
                            break;
                        }
                    }
                    if (!isInsideStructure)
                    {
                        var rockDef = GenStep_RocksFromGrid.RockDefAt(current);
                        if (rockDef != null)
                        {
                            var thing = ThingMaker.MakeThing(rockDef);
                            GenSpawn.Spawn(thing, current, map);
                        }
                    }
                }

                var cond = GameConditionMaker.MakeConditionPermanent(InternalDefOf.VQEA_AncientComplex);
                map.gameConditionManager.RegisterCondition(cond);

                var exit = map.listerThings.ThingsOfDef(exitDef).First();
                MapGenerator.PlayerStartSpot = exit.Position;
            }
            
            else
            {
                var entrance = map.listerThings.ThingsOfDef(InternalDefOf.VQEA_LockedVaultDoor).First();
                entrance.TryGetComp<CompBouncingArrow>().doBouncingArrow = true;
            }
        }
    }
}
