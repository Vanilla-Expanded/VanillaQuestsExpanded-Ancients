
using System.Collections.Generic;

using Verse;
using RimWorld;

using Verse.AI;
using Verse.Sound;


namespace VanillaQuestsExpandedAncients
{
    public class CompInteractablePowerPlant : CompInteractable
    {
        CompKickablePower cachedComp;

        public CompKickablePower compKickablePowerPlant
        {

            get
            {
                if (cachedComp is null)
                {
                    cachedComp = parent.TryGetComp<CompKickablePower>();
                }
                return cachedComp;
            }

        }

        public override bool HideInteraction
        {
            get { return compKickablePowerPlant.active; }


        }


        public override void OrderForceTarget(LocalTargetInfo target)
        {
            OrderActivation(target.Pawn);
        }

        private void OrderActivation(Pawn pawn)
        {
            Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent);
            job.count = 1;
            job.playerForced = true;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {

            AcceptanceReport acceptanceReport = CanInteract(selPawn);
            FloatMenuOption floatMenuOption = new FloatMenuOption(Props.jobString.CapitalizeFirst(), delegate
            {
                OrderActivation(selPawn);
            });
            if (!acceptanceReport.Accepted)
            {
                floatMenuOption.Disabled = true;
                floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason + ")";
            }
            yield return floatMenuOption;

        }

        protected override void OnInteracted(Pawn caster)
        {

            DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, Utils.GetMeleeDamage(caster));
            parent.TakeDamage(dinfo);
            SoundDefOf.MetalHitImportant.PlayOneShot(SoundInfo.InMap(this.parent));
            compKickablePowerPlant.RandomizeCountDownAndJuice();
            compKickablePowerPlant.FlickOn();
            compKickablePowerPlant.active = true;


        }
    }
}
