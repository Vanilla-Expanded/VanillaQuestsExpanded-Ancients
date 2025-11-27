using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;
namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(GenStep_Fog), nameof(GenStep_Fog.Generate))]
    public static class GenStep_Fog_Generate_Patch
    {
        public static bool allowGenStep = true;
        public static bool Prefix()
        {
            if (allowGenStep is true) return true;
            if (Find.GameInitData != null && Find.Scenario.AllParts.Any(part => part is ScenPart_SealedVault))
            {
                return false;
            }
            return true;
        }
    }
}
