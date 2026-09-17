using HarmonyLib;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(GeneDef), nameof(GeneDef.ConflictsWith))]
    public static class VanillaQuestsExpandedAncients_GeneDef_ConflictsWith_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(GeneDef __instance, GeneDef other, ref bool __result)
        {
            // Allow Perfect Vision and Marksman to coexist.
            bool allowCoexist = __instance == InternalDefOf.VQEA_Marksman ?
                other == InternalDefOf.VQEA_PerfectVision :
                __instance == InternalDefOf.VQEA_PerfectVision && other == InternalDefOf.VQEA_Marksman;

            if (allowCoexist)
            {
                __result = false;
            }

            return !allowCoexist;
        }
    }
}
