using RimWorld;
using VEF.Buildings;
using Verse;
using RimWorld.Planet;

namespace VanillaQuestsExpandedAncients
{
    public class CompProperties_LockedVaultDoor : CompProperties_JammedAirlock
    {
        public CompProperties_LockedVaultDoor()
        {
            compClass = typeof(CompLockedVaultDoor);
        }
    }

    public class CompLockedVaultDoor : CompJammedAirlock
    {
        protected override void OnInteracted(Pawn caster)
        {
            base.OnInteracted(caster);
            if (parent.Map?.Parent is Site site)
            {
                QuestUtility.SendQuestTargetSignals(site.questTags, "VQE_SealedVaultDoorUnlocked", site.Named("SUBJECT"));
            }
        }
    }
}
