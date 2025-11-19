using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Verse;

namespace VanillaQuestsExpandedAncients
{

    [HarmonyPatch(typeof(StatWorker), "FinalizeValue")]
    public static class VanillaQuestsExpandedAncients_StatWorker_FinalizeValue_Patch
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();

          
            var uncapStat = AccessTools.Method(typeof(VanillaQuestsExpandedAncients_StatWorker_FinalizeValue_Patch), "UncapStat");
            var stat = AccessTools.Field(typeof(StatWorker), "stat");

            for (var i = 0; i < codes.Count; i++)
            {

                if (i>3&& codes[i].opcode == OpCodes.Brfalse_S &&codes[i-1].opcode == OpCodes.Ldarg_3 && codes[i-2].opcode == OpCodes.Stind_R4&& codes[i-3].opcode == OpCodes.Conv_R4)
                {
                    yield return codes[i];

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld,stat);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, uncapStat);
                    yield return codes[i];

                }


                else yield return codes[i];
            }
        }


        public static bool UncapStat(StatDef stat,StatRequest req)
        {
                  

            if (stat == StatDefOf.MiningYield && req.Thing is Pawn pawn && pawn?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulMining)==true)
            {
                return false;

            }
            if (stat == StatDefOf.PlantHarvestYield && req.Thing is Pawn pawn2 && pawn2?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulPlants) == true)
            {
                return false;

            }
            if ((stat == InternalDefOf.CookSpeed|| stat == InternalDefOf.ButcheryFleshEfficiency) && req.Thing is Pawn pawn3 && pawn3?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulCooking) == true)
            {
                return false;

            }
            return true;
        }

    }
}