using HarmonyLib;
using VEF;
using Verse;

namespace VanillaQuestsExpandedAncients
{

    [HarmonyPatch(typeof(HediffUtility), "CanHealNaturally")]
    public static class VanillaQuestsExpandedAncients_HediffUtility_CanHealNaturally_Patch
    {
        private static void Postfix(ref bool __result, Hediff_Injury hd)
        {
            if (__result)
            {
                __result = hd.def != InternalDefOf.VQEA_Regenerating;
            }
        }
    }
}