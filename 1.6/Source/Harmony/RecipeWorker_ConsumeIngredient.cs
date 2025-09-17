using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{

    [HarmonyPatch(typeof(Bill_Medical))]
    [HarmonyPatch("Notify_BillWorkStarted")]
    public static class VanillaQuestsExpandedAncients_Bill_Medical_Notify_BillWorkStarted_Patch
    {
        public static Pawn crafter;

        [HarmonyPrefix]
        static void StoreCrafter(Pawn billDoer)
        {
          
            crafter = billDoer;

        }

    }


    [HarmonyPatch(typeof(RecipeWorker))]
    [HarmonyPatch("ConsumeIngredient")]
    public static class VanillaQuestsExpandedAncients_RecipeWorker_ConsumeIngredient_Patch
    {

        [HarmonyPrefix]
        public static void RefundMedicine(Thing ingredient, RecipeDef recipe)
        {
          
            if (Rand.Chance(0.5f)&& ingredient.def.IsMedicine&& VanillaQuestsExpandedAncients_Bill_Medical_Notify_BillWorkStarted_Patch.crafter?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulMedical) == true)
            {
                Pawn crafter = VanillaQuestsExpandedAncients_Bill_Medical_Notify_BillWorkStarted_Patch.crafter;
                Thing newProduct = ThingMaker.MakeThing(ingredient.def);
                newProduct.stackCount = ingredient.stackCount;              
                GenSpawn.Spawn(newProduct, crafter.Position, crafter.Map);
                MoteMaker.ThrowText(crafter.Position.ToVector3(), crafter.Map, "VQEA_TextMote_MedicinePreserved".Translate(), 6f);
            }
          

        }
    }

}
