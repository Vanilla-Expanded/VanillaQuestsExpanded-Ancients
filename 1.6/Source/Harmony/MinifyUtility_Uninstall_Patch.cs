using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    [HarmonyPatch(typeof(MinifyUtility), nameof(MinifyUtility.Uninstall))]
    public static class MinifyUtility_Uninstall_Patch
    {
        public static void Prefix(Thing th)
        {
            string signal = null;

            if (th.def == InternalDefOf.VQEA_SpliceframeUplink)
            {
                signal = "VQE_UplinkUninstalled";
            }
            else if (th.def == InternalDefOf.VQEA_MutagenInhibitorCore)
            {
                signal = "VQE_CoreUninstalled";
            }
            else if (th.def == InternalDefOf.VQEA_ArchitePathingArray)
            {
                signal = "VQE_ArrayUninstalled";
            }
            if (signal != null)
            {
                if (th?.Map?.Parent is Site site)
                {
                    QuestUtility.SendQuestTargetSignals(site.questTags, signal, site.Named("SUBJECT"));
                }
                else if (th?.Map?.Parent is PocketMapParent pocketMapParent && pocketMapParent.sourceMap.Parent is Site site1)
                {
                    QuestUtility.SendQuestTargetSignals(site1.questTags, signal, site1.Named("SUBJECT"));
                }
            }
        }
    }
}
