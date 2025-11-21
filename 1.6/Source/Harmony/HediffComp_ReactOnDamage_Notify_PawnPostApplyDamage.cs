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
    [HarmonyPatch(typeof(HediffComp_ReactOnDamage))]
    [HarmonyPatch("Notify_PawnPostApplyDamage")]
    public static class VanillaQuestsExpandedAncients_HediffComp_ReactOnDamage_Notify_PawnPostApplyDamage_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(HediffComp_ReactOnDamage __instance, DamageInfo dinfo)
        {
            
            if (__instance.Pawn?.genes?.HasActiveGene(InternalDefOf.VQEA_Electromagnetized)==true && dinfo.Def == DamageDefOf.EMP)
            {
              
                return false;

            }
            return true;
        }
    }
}
