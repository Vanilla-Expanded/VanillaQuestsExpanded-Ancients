using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(CompTransporter), nameof(CompTransporter.SubtractFromToLoadList))]
    public static class CompTransporter_SubtractFromToLoadList_Patch
    {
        public static void Prefix(CompTransporter __instance, ref bool sendMessageOnFinished)
        {
            if (__instance.parent is Building_PneumaticTubeLaunchPort)
            {
                sendMessageOnFinished = false;
            }
        }

        public static void Postfix(CompTransporter __instance, int __result)
        {
            if (__result > 0 && __instance.parent is Building_PneumaticTubeLaunchPort && !__instance.AnyInGroupHasAnythingLeftToLoad)
            {
                Messages.Message("VQEA_PneumaticCapsuleReady".Translate(), __instance.parent, MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}