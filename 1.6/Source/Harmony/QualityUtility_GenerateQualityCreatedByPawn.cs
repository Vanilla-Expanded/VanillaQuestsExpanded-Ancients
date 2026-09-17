using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), [typeof(Pawn), typeof(SkillDef), typeof(bool)])]
    public static class VanillaQuestsExpandedAncients_QualityUtility_GenerateQualityCreatedByPawn_Patch
    {
        public static void Postfix(Pawn pawn, SkillDef relevantSkill, ref QualityCategory __result)
        {
            if (relevantSkill != SkillDefOf.Construction)
            {
                if (pawn?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulCrafting) == true)
                {
                    __result = (QualityCategory)Math.Min((int)__result + 1, (int)QualityCategory.Legendary);
                }
            }
        }
    }
}
