using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;


namespace VanillaQuestsExpandedAncients
{


    [HarmonyPatch(typeof(InteractionWorker))]
    [HarmonyPatch("Interacted")]
    public static class VanillaQuestsExpandedAncients_InteractionWorker_Interacted_Patch
    {
        [HarmonyPostfix]
        static void VomitIfFungoid(Pawn initiator, Pawn recipient)
        {
            if (initiator.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulAnimals) == true && recipient.IsAnimal)
            {
                initiator.needs?.joy?.GainJoy(0.1f, InternalDefOf.VQEA_AnimalRelaxation);
            }

            if (initiator.genes?.HasActiveGene(InternalDefOf.VQEA_MasterfulSocial) == true && recipient.IsPrisoner && !recipient.guest.Recruitable)
            {
                if (Rand.Chance(0.2f))
                {
                    recipient.guest.Recruitable = !recipient.guest.Recruitable;
                    Messages.Message("VQEA_PawnRecruitable".Translate(recipient.NameFullColored, initiator.NameFullColored, InternalDefOf.VQEA_MasterfulSocial.label), recipient, MessageTypeDefOf.PositiveEvent, true);

                }
            }

        }
    }








}
