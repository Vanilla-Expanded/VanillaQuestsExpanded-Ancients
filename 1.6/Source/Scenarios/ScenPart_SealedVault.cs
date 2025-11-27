using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Unity.Collections;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
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

        public override void GenerateIntoMap(Map map)
        {
            base.GenerateIntoMap(map);
            if (Find.GameInitData != null)
            {
                GenStep_Fog_Generate_Patch.allowGenStep = false;
                mapParent = map.Parent;
                var rects = StructureSetGenerator.Generate(map, InternalDefOf.VQEA_SealedVaultStartStructure, Faction.OfPlayer);
                var center = new IntVec3(rects.Sum(x => x.CenterCell.x) / rects.Count, 0, rects.Sum(x => x.CenterCell.z) / rects.Count);
                var cell = rects.SelectMany(x => x.Cells).Where(x => x.Standable(map) && x.Roofed(map) && x.GetFirstBuilding(map) is null).OrderBy(x => x.DistanceTo(center)).First();

                List<Thing> thingList = new List<Thing>();
                foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
                {
                    thingList.Add(startingAndOptionalPawn);
                }

                foreach (ScenPart allPart in Find.Scenario.AllParts)
                {
                    thingList.AddRange(allPart.PlayerStartingThings());
                }

                foreach (Thing thing in thingList)
                {
                    if (thing.def.CanHaveFaction && thing.Faction != Faction.OfPlayer)
                    {
                        thing.SetFaction(Faction.OfPlayer);
                    }

                    var cellNearby = GenRadial.RadialCellsAround(cell, 15, true).Where(x => x.GetFirstThing<Thing>(map) is null && x.Standable(map) && x.Roofed(map)).RandomElement();
                    GenPlace.TryPlaceThing(thing, cellNearby, map, ThingPlaceMode.Near, extraValidator: (IntVec3 x) => x.GetFirstThing<Thing>(map) is null && x.Standable(map) && x.Roofed(map));
                }

                MapGenerator.PlayerStartSpot = cell;
                MarkSarcophagus(map, InternalDefOf.VQEA_AncientLaboratoryCasket);
                MarkSarcophagus(map, InternalDefOf.VQEA_CandidateCryptosleepCasket);
            }
        }


        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            if (Find.GameInitData != null)
            {
                GenStep_Fog_Generate_Patch.allowGenStep = true;
                new GenStep_Fog().Generate(map, default);
            }
        }
        
        

        private void MarkSarcophagus(Map map, ThingDef sarcophagusDef)
        {
            var sarcophagi = map.listerThings.ThingsOfDef(sarcophagusDef).Cast<Building_ContainmentCasket>().ToList();
            foreach (var sarcophagus in sarcophagi)
            {
                sarcophagus.isStartingScenarioBuidling = true;
            }
        }
    }
}
