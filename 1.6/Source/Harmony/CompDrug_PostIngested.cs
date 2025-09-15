using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Verse;

namespace VanillaQuestsExpandedAncients
{

    [HarmonyPatch(typeof(CompDrug), "PostIngested")]
    public static class VanillaQuestsExpandedAncients_CompDrug_PostIngested_Patch
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();

            var genedef = AccessTools.Field(typeof(Gene), "def");
            var detectNoOverdoseGene = AccessTools.Method(typeof(VanillaQuestsExpandedAncients_CompDrug_PostIngested_Patch), "DetectNoOverdoseGene");
           

            for (var i = 0; i < codes.Count; i++)
            {
               
                if ( codes[i].opcode == OpCodes.Stloc_2)
                {
                    yield return codes[i];
                  
                   
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldfld, genedef);
                    yield return new CodeInstruction(OpCodes.Call, detectNoOverdoseGene);
                    yield return new CodeInstruction(OpCodes.Stloc_0);

                }


                else yield return codes[i];
            }
        }


        public static float DetectNoOverdoseGene(GeneDef gene)
        {
            if(gene == InternalDefOf.VQEA_SubstanceImpervious)
            {
                return 0;

            }else return 1;
        }

    }
}