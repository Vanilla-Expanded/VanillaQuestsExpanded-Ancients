using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaQuestsExpandedAncients
{

    [HarmonyPatch(typeof(QualityUtility))]
    [HarmonyPatch("SendCraftNotification")]
    public static class VanillaQuestsExpandedAncients_QualityUtility_SendCraftNotification_Patch
    {

        [HarmonyPostfix]
        static void HandleCraftModifications(Thing thing, Pawn worker)
        {
            if (thing!=null &&worker?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulArtistic) == true)
            {

                CompQuality compQuality = thing.TryGetComp<CompQuality>();
                if (compQuality != null && compQuality.Quality >= QualityCategory.Masterwork)
                {

                    List<ThingDefCountClass> costlist = thing.CostListAdjusted();

                    foreach (ThingDefCountClass ingredientCount in costlist)
                    {
                        ThingDef stuff = ingredientCount.stuff;
                        Thing newProduct = ThingMaker.MakeThing(ingredientCount.thingDef, stuff);
                        newProduct.stackCount = (int)(ingredientCount.count * 0.2f);
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
