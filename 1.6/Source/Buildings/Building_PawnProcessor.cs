using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VanillaQuestsExpandedAncients
{
    public abstract class Building_PawnProcessor : Building_Enterable, IThingHolder
    {
        private int ticksRemaining;
        protected int totalProcessDuration;
        protected Sustainer sustainerWorking;
        protected Effecter progressBarEffecter;
        public int TicksRemaining => ticksRemaining;

        public bool PowerOn => this.TryGetComp<CompPowerTrader>().PowerOn;
        public override void TryAcceptPawn(Pawn p)
        {
            if ((bool)CanAcceptPawn(p))
            {
                bool num = p.DeSpawnOrDeselect();
                if (p.holdingOwner != null)
                {
                    p.holdingOwner.TryTransferToContainer(p, innerContainer);
                }
                else
                {
                    innerContainer.TryAdd(p);
                }
                if (num)
                {
                    Find.Selector.Select(p, playSound: false, forceDesignatorDeselect: false);
                }
                OnAccept(p);
            }
        }

        protected virtual void OnAccept(Pawn p)
        {
        }

        public Pawn Occupant => innerContainer.FirstOrDefault(t => t is Pawn) as Pawn;

        new public Pawn SelectedPawn
        {
            get => selectedPawn;
            protected set => selectedPawn = value;
        }

        protected abstract void FinishProcess();

        protected virtual void ProcessTick()
        {
            if (SelectedPawn != null && (SelectedPawn.Dead || SelectedPawn.MapHeld != Map))
            {
                SelectedPawn = null;
            }
        }

        protected virtual bool ShouldProcessTick()
        {
            return PowerOn;
        }

        protected virtual bool ShouldRegress => false;

        protected override void Tick()
        {
            base.Tick();
            ProcessTick();

            int previousTicks = ticksRemaining;

            if (ShouldProcessTick())
            {
                if (ticksRemaining > 0)
                {
                    ticksRemaining--;
                    if (ticksRemaining <= 0)
                    {
                        FinishProcess();
                        return;
                    }
                }
            }
            else if (ShouldRegress && ticksRemaining < totalProcessDuration)
            {
                ticksRemaining++;
            }
            if (previousTicks == totalProcessDuration && ticksRemaining < totalProcessDuration && ticksRemaining > 0)
            {
                var startSound = GetStartSound();
                if (startSound != null)
                {
                    startSound.PlayOneShot(new TargetInfo(Position, Map));
                }
            }
            if (SelectedPawn != null && ticksRemaining > 0 && ShouldShowProgressBar())
            {
                if (progressBarEffecter == null)
                {
                    progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                }
                progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                if (mote != null && totalProcessDuration > 0)
                {
                    mote.progress = 1f - (float)ticksRemaining / (float)totalProcessDuration;
                    mote.offsetZ = -0.8f;
                }
            }
            else
            {
                progressBarEffecter?.Cleanup();
                progressBarEffecter = null;
            }
            if (SelectedPawn != null && ticksRemaining > 0)
            {
                if (sustainerWorking == null || sustainerWorking.Ended)
                {
                    var operatingSound = GetOperatingSound();
                    if (operatingSound != null)
                    {
                        sustainerWorking = operatingSound.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                    }
                }
                else
                {
                    sustainerWorking.Maintain();
                }
            }
            else
            {
                sustainerWorking = null;
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            SelectedPawn = null;
            sustainerWorking = null;
            progressBarEffecter?.Cleanup();
            progressBarEffecter = null;
            base.DeSpawn(mode);
        }

        protected virtual void Reset()
        {
            ticksRemaining = 0;
            SelectedPawn = null;
        }

        protected void StartProcessing(int ticks)
        {
            ticksRemaining = ticks;
            totalProcessDuration = ticks;
        }

        protected virtual SoundDef GetOperatingSound()
        {
            return InternalDefOf.SubcoreSoftscanner_Working;
        }

        protected virtual SoundDef GetStartSound()
        {
            return InternalDefOf.SubcoreSoftscanner_Start;
        }

        protected virtual bool ShouldShowProgressBar()
        {
            return false;
        }

        protected void EjectPawn()
        {
            if (innerContainer.Any)
            {
                innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
            }
            SelectedPawn = null;
        }

        protected Command_Action CreateInsertPawnGizmo(string labelKey, string descKey, Texture2D icon, string noPawnsKey = "VQEA_NoInjectablePawns", bool disableWhenOccupied = true)
        {
            Command_Action command = new Command_Action();
            command.defaultLabel = labelKey.Translate() + "...";
            command.defaultDesc = descKey.Translate();
            command.icon = icon;
            command.action = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                IReadOnlyList<Pawn> allPawnsSpawned = Map.mapPawns.AllPawnsSpawned;
                for (int j = 0; j < allPawnsSpawned.Count; j++)
                {
                    Pawn pawn = allPawnsSpawned[j];
                    AcceptanceReport acceptanceReport = CanAcceptPawn(pawn);
                    if (!acceptanceReport.Accepted)
                    {
                        if (!acceptanceReport.Reason.NullOrEmpty())
                        {
                            list.Add(new FloatMenuOption(pawn.LabelShortCap + ": " + acceptanceReport.Reason, null));
                        }
                    }
                    else
                    {
                        list.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                        {
                            SelectPawn(pawn);
                        }));
                    }
                }
                if (!list.Any())
                {
                    list.Add(new FloatMenuOption(noPawnsKey.Translate(), null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            };
            if (disableWhenOccupied && SelectedPawn != null)
            {
                command.Disable("VQEA_WonderdocOccupied".Translate());
            }
            return command;
        }
        protected virtual IEnumerable<Gizmo>GetPawnProcessorGizmos()
        {
            if (CanCancel())
            {
                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = "Cancel".Translate();
                command_Action3.icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
                command_Action3.action = delegate
                {
                    CancelProcess();
                };
                command_Action3.activateSound = SoundDefOf.Designate_Cancel;
                yield return command_Action3;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                if (SelectedPawn != null && TicksRemaining > 0)
                {
                    Command_Action command_Action4 = new Command_Action();
                    command_Action4.defaultLabel = "DEV: Complete";
                    command_Action4.action = delegate
                    {
                        ticksRemaining = 1;
                    };
                    yield return command_Action4;
                }
            }
        }

        protected virtual bool CanCancel()
        {
            return SelectedPawn != null;
        }

        public virtual void CancelProcess()
        {
            innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
            Reset();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
            Scribe_Values.Look(ref totalProcessDuration, "totalProcessDuration", 0);
        }
    }
}
