using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace VanillaQuestsExpandedAncients
{


    [HarmonyPatch(typeof(Frame))]
    [HarmonyPatch("CompleteConstruction")]
    public static class VanillaQuestsExpandedAncients_Frame_CompleteConstruction_Patch
    {
        public static Pawn crafter;

        [HarmonyPrefix]
        static void StoreCrafter(Pawn worker)
        {
            crafter = worker;

        }

        [HarmonyPostfix]
        static void RemoveCrafter()
        {
            crafter = null;

        }
    }

    [HarmonyPatch(typeof(ThingMaker))]
    [HarmonyPatch("MakeThing")]
    public static class VanillaQuestsExpandedAncients_ThingMaker_MakeThing_Patch
    {


        [HarmonyPostfix]
        static void HandleCraftModifications(ThingDef def, Thing __result)
        {
           if(Rand.Chance(0.5f) && VanillaQuestsExpandedAncients_Frame_CompleteConstruction_Patch.crafter?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulConstruction) == true)
            {
                Pawn crafter = VanillaQuestsExpandedAncients_Frame_CompleteConstruction_Patch.crafter;
                List<ThingDefCountClass> costlist = __result.CostListAdjusted();
               
                foreach (ThingDefCountClass ingredientCount in costlist)
                {
                    ThingDef stuff = ingredientCount.stuff;
                    Thing newProduct = ThingMaker.MakeThing(ingredientCount.thingDef, stuff);
                    newProduct.stackCount = (int)(ingredientCount.count * 0.2f);
                    if (newProduct.stackCount <= 0)
                    {
                        newProduct.stackCount = 1;
                    }
                    GenSpawn.Spawn(newProduct, crafter.Position, crafter.Map);
                    
                }
                MoteMaker.ThrowText(crafter.Position.ToVector3(), crafter.Map, "VQEA_TextMote_ExceededExpectations".Translate(), 6f);
            }

        }


    }


}
