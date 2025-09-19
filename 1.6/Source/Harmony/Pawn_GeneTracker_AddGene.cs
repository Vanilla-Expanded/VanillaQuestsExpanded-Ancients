using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.AddGene))]
    [HarmonyPatch(new Type[] { typeof(Gene), typeof(bool) })]
    public static class VanillaQuestsExpandedAncients_Pawn_GeneTracker_AddGene_Patch
    {
        public static void Postfix(Pawn_GeneTracker __instance,Gene gene)
        {

            if (gene.def.exclusionTags?.Contains("MasterfulSkills")==true)
            {
                SkillRecord skill = __instance.pawn.skills.GetSkill(gene.def.passionMod.skill);
               
                skill.passion = Passion.Major;
            }
        }
    }
}
