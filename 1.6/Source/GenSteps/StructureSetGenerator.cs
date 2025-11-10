using Verse;
using RimWorld;
using RimWorld.Planet;
using KCSG;
using Verse.AI.Group;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VEF.Buildings;

namespace VanillaQuestsExpandedAncients
{
    public static class StructureSetGenerator
    {
        public static List<CellRect> Generate(Map map, StructureSetDef structureSetDef, Faction faction)
        {
            var generatedRects = new List<CellRect>();
            var mapCenter = map.Center;
            var precalculatedLayouts = new List<(StructurePatternOffset layout, KCSG.StructureLayoutDef def)>();

            foreach (var layout in structureSetDef.structureLayouts)
            {
                var matchingDefs = DefDatabase<KCSG.StructureLayoutDef>.AllDefsListForReading
                    .Where(def => Regex.IsMatch(def.defName, "^" + layout.pattern + "$"))
                    .ToList();

                if (!matchingDefs.Any())
                {
                    Log.Warning($"[VQE Ancients] No StructureLayoutDefs found matching pattern: {layout.pattern}");
                    continue;
                }

                var selectedDef = matchingDefs.RandomElement();
                precalculatedLayouts.Add((layout, selectedDef));
            }

            if (!precalculatedLayouts.Any())
            {
                return generatedRects;
            }
            int minX = int.MaxValue, minZ = int.MaxValue, maxX = int.MinValue, maxZ = int.MinValue;

            foreach (var item in precalculatedLayouts)
            {
                var layout = item.layout;
                var selectedDef = item.def;

                var relativeCenter = new IntVec3(
                    layout.offset.x * selectedDef.Sizes.x,
                    0,
                    layout.offset.z * selectedDef.Sizes.z
                );

                var rect = CellRect.CenteredOn(relativeCenter, selectedDef.Sizes);

                if (rect.minX < minX) minX = rect.minX;
                if (rect.minZ < minZ) minZ = rect.minZ;
                if (rect.maxX > maxX) maxX = rect.maxX;
                if (rect.maxZ > maxZ) maxZ = rect.maxZ;
            }
            var complexCenterX = minX + (maxX - minX) / 2;
            var complexCenterZ = minZ + (maxZ - minZ) / 2;
            var totalOffset = new IntVec3(-complexCenterX, 0, -complexCenterZ);
            foreach (var item in precalculatedLayouts)
            {
                var layout = item.layout;
                var selectedDef = item.def;
                var relativePos = new IntVec3(
                    layout.offset.x * selectedDef.Sizes.x,
                    0,
                    layout.offset.z * selectedDef.Sizes.z
                );
                var spawnPos = mapCenter + totalOffset + relativePos;

                var structureRect = CellRect.CenteredOn(spawnPos, selectedDef.Sizes);
                GenOption.GetAllMineableIn(structureRect, map);
                var spawnedThings = new List<Thing>();
                selectedDef.Generate(structureRect, map, spawnedThings, map.ParentFaction);
                foreach (var thing in spawnedThings)
                {
                    var comp = thing.TryGetComp<CompBouncingArrow>();
                    if (comp != null)
                    {
                        comp.doBouncingArrow = true;
                    }
                }
                generatedRects.Add(structureRect);
                if (layout.spawnPawns != null || layout.spawnThings != null)
                {
                    SpawnPawnsAndThings(map, structureRect, layout, faction);
                }
            }

            return generatedRects;
        }

        private static void SpawnPawnsAndThings(Map map, CellRect structureRect, StructurePatternOffset layout, Faction faction)
        {
            var walkableCells = structureRect.Cells
                .Where(cell => cell.Walkable(map) &&
                       (!layout.forceSpawnEnemiesIndoor ||
                        (cell.Roofed(map) && !cell.UsesOutdoorTemperature(map))))
                .ToList();

            if (!walkableCells.Any())
            {
                Log.Warning($"[VQE Ancients] No valid spawn cells in structure at {structureRect.CenterCell}");
                return;
            }

            var pawns = new List<Pawn>();
            if (layout.spawnPawns != null)
            {
                foreach (var spawnOption in layout.spawnPawns)
                {
                    for (var i = 0; i < spawnOption.count.RandomInRange; i++)
                    {
                        var rootCell = walkableCells.RandomElement();
                        if (!rootCell.IsValid) rootCell = structureRect.CenterCell;

                        var spawnCell = CellFinder.RandomSpawnCellForPawnNear(rootCell, map, 5);
                        if (!spawnCell.IsValid) continue;
                        var pawn = PawnGenerator.GeneratePawn(
                            spawnOption.kind, faction
                        );
                        GenSpawn.Spawn(pawn, spawnCell, map);
                        pawns.Add(pawn);
                    }
                }
            }
            if (layout.spawnThings != null)
            {
                foreach (var spawnOption in layout.spawnThings)
                {
                    for (var i = 0; i < spawnOption.count.RandomInRange; i++)
                    {
                        var rootCell = walkableCells.RandomElement();
                        if (!rootCell.IsValid) rootCell = structureRect.CenterCell;

                        var spawnCell = CellFinder.RandomSpawnCellForPawnNear(rootCell, map, 5);
                        if (!spawnCell.IsValid) continue;
                        var thing = ThingMaker.MakeThing(spawnOption.thing);
                        GenSpawn.Spawn(thing, spawnCell, map);
                        if (thing is Hive hive)
                        {
                            hive.SetFaction(Faction.OfInsects);
                        }
                        else if (thing.def.CanHaveFaction)
                        {
                            thing.SetFaction(faction);
                        }
                    }
                }
            }
            if (pawns.Any())
            {
                var defenseCenter = walkableCells.RandomElement();
                var lordJob = new LordJob_DefendPoint(defenseCenter);
                LordMaker.MakeNewLord(faction, lordJob, map, pawns);
            }
        }
    }
}
