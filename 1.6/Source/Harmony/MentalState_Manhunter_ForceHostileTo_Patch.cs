using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(MentalState_Manhunter), "ForceHostileTo", new Type[] { typeof(Thing) })]
    public static class MentalState_Manhunter_ForceHostileTo_Patch
    {
        public static void Postfix(MentalState_Manhunter __instance, ref bool __result, Thing t)
        {
            if (__result && __instance.pawn.Faction == Faction.OfAncientsHostile && t is Pawn pawn && pawn.Faction == Faction.OfAncientsHostile && __instance.pawn.Map?.Parent?.GetAssociatedPart<QuestPart_AncientLab>() != null)
            {
                __result = false;
            }
        }
    }
}
