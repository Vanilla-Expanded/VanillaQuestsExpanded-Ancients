using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;



namespace VanillaQuestsExpandedAncients
{

    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("MakeRecipeProducts")]
    public static class VanillaQuestsExpandedAncients_GenRecipe_MakeRecipeProducts_Patch
    {

        public static List<Thing> cachedIngredients = new List<Thing>();

        [HarmonyPrefix]
        static void StoreIngredients(List<Thing> ingredients)
        {
            cachedIngredients.Clear();
            cachedIngredients = ingredients;
        }
    }



    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("PostProcessProduct")]
    public static class VanillaQuestsExpandedAncients_GenRecipe_PostProcessProduct_Patch
    {
        [HarmonyPostfix]
        static void HandleCraftModifications(Thing product, RecipeDef recipeDef, Pawn worker)
        {


            if (worker?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulArtistic) == true)
            {

                CompQuality compQuality = product?.TryGetComp<CompQuality>();
                if (compQuality != null && compQuality.Quality >= QualityCategory.Masterwork)
                {
                    if (recipeDef?.workSkill == null)
                    {
                        Log.Error(recipeDef + " needs workSkill because it creates a product with a quality.");
                    }
                    if (!VanillaQuestsExpandedAncients_GenRecipe_MakeRecipeProducts_Patch.cachedIngredients.NullOrEmpty())
                    {
                        foreach (Thing ingredient in VanillaQuestsExpandedAncients_GenRecipe_MakeRecipeProducts_Patch.cachedIngredients)
                        {
                            ThingDef stuff = ingredient.Stuff;
                            Thing newProduct = ThingMaker.MakeThing(ingredient.def, stuff);
                            newProduct.stackCount = (int)(ingredient.stackCount * 0.2f);
                            if (newProduct.stackCount <= 0)
                            {
                                newProduct.stackCount = 1;
                            }
                            GenSpawn.Spawn(newProduct, worker.Position, worker.Map);

                        }
                    }
                    
                    
                    

                }
            }
        }
    }
}
