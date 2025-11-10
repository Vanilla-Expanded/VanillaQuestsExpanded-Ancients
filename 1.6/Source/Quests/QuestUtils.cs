using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VanillaQuestsExpandedAncients
{
    public static class QuestUtils
    {
        public static List<PawnKindDef> GeneratePawnKindList(Faction faction, float points, Site site)
        {
            PawnGroupMakerParms pawnGroupMakerParms = GetParms(faction, points, site);
            var minPoints = faction.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind, pawnGroupMakerParms);
            points = minPoints < float.MaxValue ? Mathf.Max(points, minPoints) : points;
            pawnGroupMakerParms.points = points;
            List<PawnKindDef> generatedPawns = new List<PawnKindDef>();
            while (generatedPawns.Any() is false && points < 99999)
            {
                points += 50f;
                pawnGroupMakerParms.points = points;
                generatedPawns = GeneratePawnKinds(pawnGroupMakerParms, false).ToList();
            }
            return generatedPawns;
        }
        public static IEnumerable<PawnKindDef> GeneratePawnKinds(PawnGroupMakerParms parms, bool warnOnZeroResults = true)
        {
            if (parms.groupKind == null)
            {
                Log.Error("Tried to generate pawns with null pawn group kind def. parms=" + parms);
                yield break;
            }
            if (parms.faction == null)
            {
                Log.Error("Tried to generate pawn kinds with null faction. parms=" + parms);
                yield break;
            }
            if (parms.faction.def.pawnGroupMakers.NullOrEmpty())
            {
                Log.Error(string.Concat("Faction ", parms.faction, " of def ", parms.faction.def, " has no PawnGroupMakers."));
                yield break;
            }
            if (!PawnGroupMakerUtility.TryGetRandomPawnGroupMaker(parms, out var pawnGroupMaker))
            {
                yield break;
            }
            foreach (PawnKindDef item in pawnGroupMaker.GeneratePawnKindsExample(parms))
            {
                yield return item;
            }
        }
        private static PawnGroupMakerParms GetParms(Faction faction, float points, Site site)
        {
            return new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = site.Tile,
                faction = faction,
                points = points,
                raidStrategy = RaidStrategyDefOf.ImmediateAttack
            };
        }

        public static string FormatPawnListToString(List<PawnKindDef> pawns)
        {
            if (pawns == null || !pawns.Any())
            {
                return "";
            }
            return pawns.GroupBy(p => p).Select(group => $"{group.Count()} {group.Key.label}").ToCommaList();
        }

        public static T GetAssociatedPart<T>(this MapParent parent) where T : QuestPart_Site
        {
            foreach (var quest in Find.QuestManager.QuestsListForReading.Where(x => x.State == QuestState.Ongoing))
            {
                List<QuestPart> questParts = quest.PartsListForReading;
                for (var i = 0; i < questParts.Count; i++)
                {
                    if (questParts[i] is T questSite)
                    {
                        if (questSite.mapParent == parent)
                        {
                            return questSite;
                        }
                        else if (parent is PocketMapParent pocketMapParent && pocketMapParent.sourceMap.Parent == questSite.mapParent)
                        {
                            return questSite;
                        }
                    }
                }
            }
            return null;
        }
    }
}
