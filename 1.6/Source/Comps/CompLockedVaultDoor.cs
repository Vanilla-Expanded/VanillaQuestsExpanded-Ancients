using RimWorld;
using VEF.Buildings;
using Verse;

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
            Find.SignalManager.SendSignal(new Signal("VQE_SealedVaultDoorUnlocked", parent.Named("SUBJECT")));
        }
    }
}
