using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HotSwappable]
    [HarmonyPatch(typeof(GeneDef), "GetDescriptionFull")]
    public static class GeneDef_GetDescriptionFull_Patch
    {
        public static void Postfix(GeneDef __instance, ref string __result)
        {
            if (__instance.defName?.Contains("VQEA_Masterful") == true)
            {
                var originalPassion = "PassionModDrop".Translate(__instance.passionMod.skill);
                var newPassion = "VQEA_AddsBurningPassionIn".Translate(__instance.passionMod.skill);
                if (__result.Contains(originalPassion))
                {
                    __result = __result.Replace(originalPassion, newPassion);
                }
            }
        }
    }
}
