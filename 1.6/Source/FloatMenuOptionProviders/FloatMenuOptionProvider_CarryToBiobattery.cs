
using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;
namespace VanillaQuestsExpandedAncients
{
    public class FloatMenuOptionProvider_CarryToBiobattery : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool RequiresManipulation => true;

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (!clickedPawn.Downed)
            {
                return null;
            }
            if (!context.FirstSelectedPawn.CanReserveAndReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
            {
                return null;
            }
            if (CompBioBattery.FindBatteryFor(clickedPawn, context.FirstSelectedPawn, ignoreOtherReservations: true) == null)
            {
                return null;
            }
            TaggedString taggedString = "VQEA_CarryToBioBattery".Translate(clickedPawn.LabelCap);
            if (clickedPawn.IsQuestLodger())
            {
                return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString + " (" + "CryptosleepCasketGuestsNotAllowed".Translate() + ")", null, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
            }
            if (clickedPawn.GetExtraHostFaction() != null)
            {
                return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString + " (" + "CryptosleepCasketGuestPrisonersNotAllowed".Translate() + ")", null, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
            }
            Action action = delegate
            {
                Thing building_Battery = CompBioBattery.FindBatteryFor(clickedPawn, context.FirstSelectedPawn);
                if (building_Battery == null)
                {
                    building_Battery = CompBioBattery.FindBatteryFor(clickedPawn, context.FirstSelectedPawn, ignoreOtherReservations: true);
                }
                if (building_Battery == null)
                {
                    Messages.Message("CannotCarryToCryptosleepCasket".Translate() + ": " + "NoCryptosleepCasket".Translate(), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
                }
                else
                {
                    Job job = JobMaker.MakeJob(InternalDefOf.VQEA_CarryToBioBattery, clickedPawn, building_Battery);
                    job.count = 1;
                    context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                
                }
            };
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, action, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
        }
    }
}