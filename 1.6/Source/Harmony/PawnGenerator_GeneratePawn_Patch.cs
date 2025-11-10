using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    public static class PawnGenerator_GeneratePawn_Patch
    {
        public static void Postfix(Pawn __result, PawnGenerationRequest request)
        {
            var extension = __result?.kindDef?.GetModExtension<PawnKindExtension_Experiment>();
            if (extension != null && __result.genes != null)
            {
                if (extension.architeGeneCount > 0)
                {
                    int added = 0;
                    var candidates = DefDatabase<GeneDef>.AllDefs.Where(x => x.biostatArc > 0).InRandomOrder();
                    foreach (var geneDef in candidates)
                    {
                        if (added >= extension.architeGeneCount) break;
                        if (!__result.genes.HasActiveGene(geneDef)
                            && !__result.genes.GenesListForReading.Any(g => g.def.ConflictsWith(geneDef))
                            && (geneDef.prerequisite == null || __result.genes.HasActiveGene(geneDef.prerequisite)))
                        {
                            __result.genes.AddGene(geneDef, xenogene: false);
                            added++;
                        }
                    }
                }

                if (extension.metabolismGeneCount > 0)
                {
                    int added = 0;
                    var candidates = DefDatabase<GeneDef>.AllDefs
                        .Where(g => g.biostatMet >= extension.minMetabolismForGene
                                 && g.biostatMet > 0
                                 && g.biostatArc <= 0)
                        .InRandomOrder();

                    foreach (var geneDef in candidates)
                    {
                        if (added >= extension.metabolismGeneCount) break;
                        if (!__result.genes.HasActiveGene(geneDef)
                            && !__result.genes.GenesListForReading.Any(g => g.def.ConflictsWith(geneDef))
                            && (geneDef.prerequisite == null || __result.genes.HasActiveGene(geneDef.prerequisite)))
                        {
                            __result.genes.AddGene(geneDef, xenogene: false);
                            added++;
                        }
                    }
                }
            }
        }
    }
}
