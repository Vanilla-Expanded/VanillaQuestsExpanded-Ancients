using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    public static class VanillaQuestsExpandedAncients_Frame_CompleteConstruction_Patch
    {
        static void Postfix(Frame __instance, Pawn worker)
        {
            if (Rand.Chance(0.5f) && worker?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulConstruction) == true)
            {
                List<ThingDefCountClass> costlist = __instance.TotalMaterialCost();

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

                MoteMaker.ThrowText(worker.Position.ToVector3(), worker.Map, "VQEA_TextMote_ExceededExpectations".Translate(), 6f);
            }
        }
    }
}
