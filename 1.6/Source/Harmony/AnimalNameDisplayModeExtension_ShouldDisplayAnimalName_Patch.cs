using HarmonyLib;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(AnimalNameDisplayModeExtension), "ShouldDisplayAnimalName")]
    public static class AnimalNameDisplayModeExtension_ShouldDisplayAnimalName_Patch
    {
        public static void Postfix(Pawn animal, ref bool __result)
        {
            if (animal.kindDef == InternalDefOf.VQEA_Splicefiend || animal.kindDef == InternalDefOf.VQEA_Splicehulk || animal.kindDef == InternalDefOf.VQEA_Spliceling)
            {
                __result = true;
            }
        }
    }


}
