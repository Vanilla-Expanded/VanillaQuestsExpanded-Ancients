using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Verse.AI;
using Verse;
using RimWorld;


namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch]
    public static class VanillaQuestsExpandedAncients_MentalWorker_Patches
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var targetMethod = AccessTools.DeclaredMethod(typeof(MentalBreakWorker), "TryStart");
            yield return targetMethod;
            foreach (var subclass in typeof(MentalBreakWorker).AllSubclasses())
            {
                var method = AccessTools.DeclaredMethod(subclass, "TryStart");
                if (method != null)
                {
                    yield return method;
                }
            }
        }

        public static bool Prefix(ref bool __result, MentalBreakWorker __instance, Pawn __0, string __1, bool __2)
        {
            if (__0.genes?.HasActiveGene(InternalDefOf.VQEA_Serene) == true)
            {

                __result = false;
                return false;


            }
            return true;
        }
    }
}
