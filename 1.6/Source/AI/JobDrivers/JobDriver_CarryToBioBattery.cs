using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaQuestsExpandedAncients
{
    public class JobDriver_CarryToBioBattery : JobDriver
    {
        private const TargetIndex TakeeInd = TargetIndex.A;

        private const TargetIndex BatteryInd = TargetIndex.B;

        private Pawn Takee => job.GetTarget(TakeeInd).Pawn;

        private CompBioBattery Pod => job.GetTarget(BatteryInd).Thing.TryGetComp<CompBioBattery>();

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Pod.parent, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TakeeInd);
            this.FailOnDestroyedOrNull(BatteryInd);
            this.FailOnAggroMentalState(TakeeInd);
            var goToTakee = Toils_Goto.GotoThing(TakeeInd, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TakeeInd).FailOnDespawnedNullOrForbidden(BatteryInd)
                .FailOn(() => Takee.IsColonist && !Takee.Downed).FailOnSomeonePhysicallyInteracting(TakeeInd);
            var startCarryingTakee = Toils_Haul.StartCarryThing(TakeeInd);
            var goToThing = Toils_Goto.GotoThing(BatteryInd, PathEndMode.InteractionCell);
            yield return Toils_Jump.JumpIf(goToThing, () => pawn.IsCarryingPawn(Takee));
            yield return goToTakee;
            yield return startCarryingTakee;
            yield return goToThing;
            yield return PrepareToEnterToil(BatteryInd);
            yield return new Toil
            {
                initAction = delegate { Pod.InsertPawn(Takee); },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        public static Toil PrepareToEnterToil(TargetIndex podIndex)
        {
            var prepare = Toils_General.Wait(JobDriver_EnterBiosculpterPod.EnterPodDelay);
            prepare.FailOnCannotTouch(podIndex, PathEndMode.InteractionCell);
            prepare.WithProgressBarToilDelay(podIndex);
            return prepare;
        }
    }
}