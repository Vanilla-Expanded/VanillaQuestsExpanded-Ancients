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
    public static class VanillaQuestsExpandedAncients_DamageWorker_AddInjury_ChooseHitPart_Patches
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var targetMethod = AccessTools.DeclaredMethod(typeof(DamageWorker_AddInjury), "ChooseHitPart");
            yield return targetMethod;
            foreach (var subclass in typeof(DamageWorker_AddInjury).AllSubclasses())
            {
                var method = AccessTools.DeclaredMethod(subclass, "ChooseHitPart");
                if (method != null)
                {
                    yield return method;
                }
            }
        }

        public static bool Prefix(ref BodyPartRecord __result, DamageInfo dinfo, Pawn pawn)
        {
            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker!=null && !dinfo.Def.isRanged && attacker.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulMelee) == true)
            {
                
                var vitalParts = pawn.health.hediffSet.GetNotMissingParts(dinfo.Height, BodyPartDepth.Inside).Where(part => part.def== BodyPartDefOf.Heart || part.def == InternalDefOf.Brain || part.def == InternalDefOf.ArtificialBrain).ToList();
                if (vitalParts.TryRandomElementByWeight(x => x.coverageAbs * x.def.GetHitChanceFactorFor(dinfo.Def), out __result)) return false;
                if (vitalParts.TryRandomElementByWeight(x => x.coverageAbs, out __result)) return false;

            }
            if (attacker != null && dinfo.Def.isRanged && attacker.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulShooting) == true)
            {
               
                var vitalParts = pawn.health.hediffSet.GetNotMissingParts().Where(part => part.def == BodyPartDefOf.Head || part.def == InternalDefOf.Reactor || part.def == InternalDefOf.InsectHead).ToList();
                if (vitalParts.TryRandomElementByWeight(x => x.coverageAbs * x.def.GetHitChanceFactorFor(dinfo.Def), out __result)) return false;
                if (vitalParts.TryRandomElementByWeight(x => x.coverageAbs, out __result)) return false;

            }
            return true;
        }
    }
}
