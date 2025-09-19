using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaQuestsExpandedAncients
{

    [HarmonyPatch(typeof(SurgeryOutcome_Failure))]
    [HarmonyPatch("Apply")]
    public static class VanillaQuestsExpandedAncients_SurgeryOutcome_Failure_Apply_Patch
    {

        [HarmonyPrefix]
        static bool RemoveFailures(Pawn surgeon, ref bool __result)
        {
            if (surgeon?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulMedical) == true)
            {
                __result = false;
                return false;
            }
            return true;

        }
    }

}
