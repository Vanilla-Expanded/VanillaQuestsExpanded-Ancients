using HarmonyLib;
using RimWorld;

using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.AddictionChanceFactor))]
    public static class VanillaQuestsExpandedAncients_Pawn_GeneTracker_AddictionChanceFactor_Patch
    {
        public static void Postfix(Pawn_GeneTracker __instance, ref float __result)
        {

            if (__instance.HasActiveGene(InternalDefOf.VQEA_SubstanceImpervious))
            {
                __result = 0;
            }
        }
    }
}
