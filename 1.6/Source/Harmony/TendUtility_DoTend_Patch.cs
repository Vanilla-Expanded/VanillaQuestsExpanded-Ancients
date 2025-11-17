using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(TendUtility), "DoTend")]
    public static class TendUtility_DoTend_Patch
    {
        public static void Prefix(Pawn doctor, Pawn patient, Medicine medicine, out int? __state)
        {
            __state = medicine?.stackCount;
            if (medicine != null && doctor?.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulMedical) == true)
            {
                medicine.stackCount += 1;
            }
        }

        public static void Postfix(Medicine medicine, int? __state)
        {
            if (medicine != null && medicine.stackCount - 1 == __state)
            {
                medicine.stackCount--;
            }
        }
    }
}
