using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEF.Genes;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public static class Utils
    {


        public static float GetMeleeDamage(Pawn pawn)
        {

            if (pawn == null)
            {
                return 0f;
            }
            List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList(terrainTools: false);
            if (updatedAvailableVerbsList.Count == 0)
            {
                return 0f;
            }
            float num = 0f;
            for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
            {
                if (updatedAvailableVerbsList[i].IsMeleeAttack)
                {
                    num += updatedAvailableVerbsList[i].GetSelectionWeight(null);
                }
            }
            if (num == 0f)
            {
                return 0f;
            }
            float num2 = 0f;
            for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
            {
                if (updatedAvailableVerbsList[j].IsMeleeAttack)
                {
                    num2 += updatedAvailableVerbsList[j].GetSelectionWeight(null) / num * updatedAvailableVerbsList[j].verb.verbProps.AdjustedMeleeDamageAmount(updatedAvailableVerbsList[j].verb, pawn);
                }
            }
            return num2;
        }

        public static bool IsValidGeneForInjection(GeneDef geneDef)
        {
            if (geneDef.displayCategory?.defName == "VRE_Morphs" || ModCompatibility.IsAndroidGene(geneDef))
            {
                return false;
            }

            var extension = geneDef.GetModExtension<GeneExtension>();
            if (extension != null && extension.disableGeneExtraction)
            {
                return false;
            }

            foreach (var blacklistDef in DefDatabase<InjectionBlacklistDef>.AllDefs)
            {
                if (blacklistDef.blacklistedGenes.Contains(geneDef.defName))
                {
                    return false;
                }

                if (geneDef.exclusionTags != null)
                {
                    foreach (var tag in geneDef.exclusionTags)
                    {
                        if (blacklistDef.blacklistedExclusionTags.Contains(tag))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
