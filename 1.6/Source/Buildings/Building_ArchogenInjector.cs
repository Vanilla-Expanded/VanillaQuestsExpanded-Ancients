using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using System.Text;
using Verse.Sound;

namespace VanillaQuestsExpandedAncients
{
    public enum ArchiteInjectorState
    {
        Inactive,
        WaitingForCapsule,
        WaitingForPawn,
        Injecting,
        Complete
    }

    [StaticConstructorOnStartup]
    public class Building_ArchogenInjector : Building_Enterable, IThingHolder
    {
        private bool init;
        public int ticksRemaining;
        private int totalInjectionTime;
        private ArchiteInjectionOutcomeDef outcome;
        private Effecter effectStart;
        private Sustainer sustainerWorking;
        private Effecter progressBarEffecter;
        private Mote workingMote;
        private bool debugDisableNeedForIngredients;
        private GeneDef generatedArchiteGene;
        private GeneDef generatedSideEffectGene;

        private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
        public static readonly CachedTexture InsertPersonIcon = new CachedTexture("UI/Icons/InsertPersonSubcoreScanner");
        public CachedTexture InitIcon = new CachedTexture("UI/Gizmos/InsertPawn");
        public bool PowerOn => this.TryGetComp<CompPowerTrader>().PowerOn;
        public override Vector3 PawnDrawOffset => IntVec3.West.RotatedBy(Rotation).ToVector3() / def.size.x;

        public Pawn Occupant => innerContainer.FirstOrDefault(t => t is Pawn) as Pawn;

        new public Pawn SelectedPawn => selectedPawn;

        public bool AllRequiredIngredientsLoaded
        {
            get
            {
                if (!debugDisableNeedForIngredients)
                {
                    if (GetRequiredCountOf(ThingDefOf.ArchiteCapsule) > 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public ArchiteInjectorState State
        {
            get
            {
                if (!init || !PowerOn)
                {
                    return ArchiteInjectorState.Inactive;
                }
                if (!AllRequiredIngredientsLoaded)
                {
                    return ArchiteInjectorState.WaitingForCapsule;
                }
                if (Occupant == null)
                {
                    return ArchiteInjectorState.WaitingForPawn;
                }
                if (ticksRemaining > 0)
                {
                    return ArchiteInjectorState.Injecting;
                }
                return ArchiteInjectorState.Complete;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!ModLister.CheckBiotech("archogen injector"))
            {
                Destroy();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            progressBarEffecter?.Cleanup();
            progressBarEffecter = null;
            effectStart?.Cleanup();
            effectStart = null;
            workingMote = null;
            sustainerWorking = null;
            base.DeSpawn(mode);
        }

        public int GetRequiredCountOf(ThingDef thingDef)
        {
            if (thingDef == ThingDefOf.ArchiteCapsule)
            {
                if (innerContainer.Any(t => t.def == ThingDefOf.ArchiteCapsule))
                {
                    return 0;
                }
                return 1;
            }
            return 0;
        }

        public override AcceptanceReport CanAcceptPawn(Pawn pawn)
        {
            if (!pawn.IsColonist && !pawn.IsSlaveOfColony && !pawn.IsPrisonerOfColony)
            {
                return false;
            }
            if (selectedPawn != null && selectedPawn != pawn)
            {
                return false;
            }
            if (!PowerOn)
            {
                return "CannotUseNoPower".Translate();
            }
            if (State != ArchiteInjectorState.WaitingForPawn)
            {
                switch (State)
                {
                    case ArchiteInjectorState.Inactive:
                        return "VQEA_ArchogenInjectorNotInit".Translate();
                    case ArchiteInjectorState.WaitingForCapsule:
                        return "VQEA_ArchogenInjectorWaitingForCapsule".Translate();
                    case ArchiteInjectorState.Injecting:
                    case ArchiteInjectorState.Complete:
                        return "VQEA_ArchogenInjectorOccupied".Translate();
                }
            }
            else
            {
                if (pawn.IsQuestLodger())
                {
                    return "CryptosleepCasketGuestsNotAllowed".Translate();
                }
                if (pawn.DevelopmentalStage.Baby())
                {
                    return "SubcoreScannerBabyNotAllowed".Translate();
                }
                if (!pawn.RaceProps.Humanlike)
                {
                    return false;
                }
                if (pawn.health.hediffSet.HasHediff(HediffDefOf.XenogerminationComa))
                {
                    return "InXenogerminationComa".Translate();
                }
            }
            return true;
        }

        public override void TryAcceptPawn(Pawn pawn)
        {
            if ((bool)CanAcceptPawn(pawn))
            {
                bool num = pawn.DeSpawnOrDeselect();
                if (pawn.holdingOwner != null)
                {
                    pawn.holdingOwner.TryTransferToContainer(pawn, innerContainer);
                }
                else
                {
                    innerContainer.TryAdd(pawn);
                }
                if (num)
                {
                    Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
                }
            }
        }

        public bool CanAcceptIngredient(Thing thing)
        {
            return GetRequiredCountOf(thing.def) > 0;
        }

        public void EjectContents()
        {
            if (Occupant != null)
            {
                FinishInjection();
            }
            innerContainer.RemoveAll(x => x.def == ThingDefOf.ArchiteCapsule);
            innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
            selectedPawn = null;
            init = false;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }
            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }
            AcceptanceReport acceptanceReport = CanAcceptPawn(selPawn);
            if (acceptanceReport.Accepted)
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("EnterBuilding".Translate(this), delegate
                {
                    SelectPawn(selPawn);
                }), selPawn, this);
            }
            else if (!acceptanceReport.Reason.NullOrEmpty())
            {
                yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
            }
        }

        public static bool WasLoadingCancelled(Thing thing)
        {
            if (thing is Building_ArchogenInjector archogenInjector && !archogenInjector.init)
            {
                return true;
            }
            return false;
        }

        protected override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(250))
            {
                var powerComp = this.TryGetComp<CompPowerTrader>();
                if (State == ArchiteInjectorState.Injecting)
                {
                    powerComp.PowerOutput = 0f - powerComp.Props.PowerConsumption;
                }
                else
                {
                    powerComp.PowerOutput = 0f - powerComp.Props.idlePowerDraw;
                }
            }

            if (State == ArchiteInjectorState.Injecting)
            {
                if (PowerOn)
                {
                    ticksRemaining--;
                }
                else
                {
                    if (ticksRemaining < totalInjectionTime)
                    {
                        ticksRemaining++;
                    }
                }
                if (ticksRemaining <= 0)
                {
                    EjectContents();
                }

                if (progressBarEffecter == null)
                {
                    progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                }
                progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                if (mote != null)
                {
                    mote.progress = 1f - (float)ticksRemaining / (float)totalInjectionTime;
                    mote.offsetZ = -0.8f;
                }
                if (sustainerWorking == null || sustainerWorking.Ended)
                {
                    sustainerWorking = SoundDefOf.GeneExtractor_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                }
                else
                {
                    sustainerWorking.Maintain();
                }
            }
            else
            {
                progressBarEffecter?.Cleanup();
                progressBarEffecter = null;
                sustainerWorking = null;
            }

            if (Occupant != null && Occupant.Dead)
            {
                EjectContents();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (!init)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "VQEA_StartInjection".Translate();
                command_Action.defaultDesc = "VQEA_StartInjectionDesc".Translate();
                command_Action.icon = InitIcon.Texture;
                command_Action.action = delegate
                {
                    init = true;
                };
                command_Action.activateSound = SoundDefOf.Tick_Tiny;
                yield return command_Action;
            }
            else if (selectedPawn == null)
            {
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "VQEA_InsertPerson".Translate() + "...";
                command_Action2.defaultDesc = "VQEA_InsertPersonDesc".Translate();
                command_Action2.icon = InsertPersonIcon.Texture;
                command_Action2.action = delegate
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
                                list.Add(new FloatMenuOption(pawn.LabelShortCap + ": " + acceptanceReport.Reason, null, pawn, Color.white));
                            }
                        }
                        else
                        {
                            list.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                            {
                                SelectPawn(pawn);
                                Find.WindowStack.Add(new Window_ArchiteInjection(this));
                            }, pawn, Color.white));
                        }
                    }
                    if (!list.Any())
                    {
                        list.Add(new FloatMenuOption("VQEA_NoInjectablePawns".Translate(), null));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                };
                if (!PowerOn)
                {
                    command_Action2.Disable("NoPower".Translate().CapitalizeFirst());
                }
                else if (State == ArchiteInjectorState.WaitingForCapsule)
                {
                    command_Action2.Disable("VQEA_ArchogenInjectorWaitingForCapsule".Translate());
                }
                yield return command_Action2;
            }
            if (init)
            {
                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = "CommandCancelLoad".Translate();
                command_Action3.defaultDesc = "CommandCancelLoadDesc".Translate();
                command_Action3.icon = CancelIcon;
                command_Action3.action = delegate
                {
                    EjectContents();
                };
                command_Action3.activateSound = SoundDefOf.Designate_Cancel;
                yield return command_Action3;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                if (State == ArchiteInjectorState.Injecting)
                {
                    Command_Action command_Action4 = new Command_Action();
                    command_Action4.defaultLabel = "DEV: Complete";
                    command_Action4.action = delegate
                    {
                        ticksRemaining = 1;
                    };
                    yield return command_Action4;
                }

                Command_Action command_Action5 = new Command_Action();
                command_Action5.defaultLabel = (debugDisableNeedForIngredients ? "DEV: Enable Ingredients" : "DEV: Disable Ingredients");
                command_Action5.action = delegate
                {
                    debugDisableNeedForIngredients = !debugDisableNeedForIngredients;
                };
                yield return command_Action5;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            switch (State)
            {
                case ArchiteInjectorState.WaitingForCapsule:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("VQEA_ArchogenInjectorWaitingForCapsule".Translate());
                    break;
                case ArchiteInjectorState.WaitingForPawn:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("VQEA_ArchogenInjectorWaitingForPawn".Translate());
                    break;
                case ArchiteInjectorState.Injecting:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("VQEA_ArchogenInjectorInjecting".Translate(ticksRemaining.ToStringTicksToPeriod()));
                    break;
                case ArchiteInjectorState.Complete:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("VQEA_ArchogenInjectorComplete".Translate());
                    break;
            }
            return stringBuilder.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref init, "init", defaultValue: false);
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
            Scribe_Values.Look(ref totalInjectionTime, "totalInjectionTime", 0);
            Scribe_Defs.Look(ref outcome, "outcome");
        }

        public void StartInjection()
        {
            CalculateOutcome();
            totalInjectionTime = GetExpectedInfusionDuration();
            ticksRemaining = totalInjectionTime;
        }

        private void FinishInjection()
        {
            if (Occupant != null)
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(new TargetInfo(Position, Map));
                if (outcome == InternalDefOf.VQEA_ArchiteInjection_Success)
                {
                    HandleSuccessOutcome(Occupant);
                }
                else if (outcome == InternalDefOf.VQEA_ArchiteInjection_Rejection)
                {
                    HandleRejectionOutcome(Occupant);
                }
                else
                {
                    HandleMutationOutcome(Occupant, outcome.pawnKind);
                }
            }
        }

        private List<GeneDef> GetFilteredGenes(Pawn pawn, Func<GeneDef, bool> additionalFilter)
        {
            return DefDatabase<GeneDef>.AllDefs.Where(g =>
                additionalFilter(g) &&
                (g.prerequisite == null || pawn.genes.HasActiveGene(g.prerequisite)) &&
                !pawn.genes.HasActiveGene(g) &&
                !pawn.genes.GenesListForReading.Any(existing => existing.def.ConflictsWith(g))
            ).ToList();
        }

        private void HandleSuccessOutcome(Pawn pawn)
        {
            NamedArgument pawnArg = Occupant.Named("PAWN");
            NamedArgument architeGeneArg = this.generatedArchiteGene.Named("ARCHITEGENE");
            NamedArgument sideEffectGeneArg = this.generatedSideEffectGene.Named("SIDEEFFECTGENE");
            Find.LetterStack.ReceiveLetter("VQEA_LetterLabel_Success".Translate(), "VQEA_LetterDesc_Success".Translate(pawnArg, architeGeneArg, sideEffectGeneArg), LetterDefOf.PositiveEvent, Occupant);

            var architeGenes = GetFilteredGenes(pawn, g => g.biostatArc > 0);
            if (architeGenes.Any())
            {
                if (HasLinkedFacility(InternalDefOf.VQEA_TraitSelectionPrism))
                {
                    var gene1 = architeGenes.RandomElement();
                    var availableGenes = architeGenes.ToList();
                    availableGenes.Remove(gene1);
                    var gene2 = availableGenes.Any() ? availableGenes.RandomElement() : gene1;

                    Find.WindowStack.Add(new Dialog_MessageBox("VQEA_ChooseArchiteGene".Translate(), gene1.LabelCap, () => { pawn.genes.AddGene(gene1, xenogene: true); generatedArchiteGene = gene1; ContinueWithAdditionalGenesAndComa(pawn); }, gene2.LabelCap, () => { pawn.genes.AddGene(gene2, xenogene: true); generatedArchiteGene = gene2; ContinueWithAdditionalGenesAndComa(pawn); }));
                    return;
                }
                else
                {
                    var selectedArchiteGene = architeGenes.RandomElement();
                    pawn.genes.AddGene(selectedArchiteGene, xenogene: true);
                    generatedArchiteGene = selectedArchiteGene;
                }
            }
            ContinueWithAdditionalGenesAndComa(pawn);
        }

        private void ContinueWithAdditionalGenesAndComa(Pawn pawn)
        {
            var architeGenes = GetFilteredGenes(pawn, g => g.biostatArc > 0);

            if (HasLinkedFacility(InternalDefOf.VQEA_ArchitePathingArray) && Rand.Chance(0.25f))
            {
                var remainingArchiteGenes = architeGenes.Where(g => !pawn.genes.HasActiveGene(g)).ToList();
                if (remainingArchiteGenes.Any())
                {
                    var secondArchiteGene = remainingArchiteGenes.RandomElement();
                    pawn.genes.AddGene(secondArchiteGene, xenogene: true);
                }
            }
            FindAndAddSideEffectGene(pawn);
            if (HasLinkedFacility(InternalDefOf.VQEA_ArchitePathingArray) && Rand.Chance(0.50f))
            {
                FindAndAddSecondNegativeGene(pawn);
            }
            ApplyInjectionComa(pawn);
        }

        private void FindAndAddSecondNegativeGene(Pawn pawn)
        {
            var negativeGenes = GetFilteredGenes(pawn, g => g.biostatArc <= 0 && g.biostatMet <= 0);
            if (negativeGenes.Any())
            {
                if (HasLinkedFacility(InternalDefOf.VQEA_AberrationRedirector) && negativeGenes.Count >= 2)
                {
                    var gene1 = negativeGenes.RandomElement();
                    var availableGenes = negativeGenes.ToList();
                    availableGenes.Remove(gene1);
                    var gene2 = availableGenes.Any() ? availableGenes.RandomElement() : gene1;

                    Find.WindowStack.Add(new Dialog_MessageBox("VQEA_ChooseSideEffectGene".Translate(), gene1.LabelCap, () => { pawn.genes.AddGene(gene1, xenogene: true); }, gene2.LabelCap, () => { pawn.genes.AddGene(gene2, xenogene: true); }));
                    return;
                }
                else
                {
                    var selectedNegativeGene = negativeGenes.RandomElement();
                    pawn.genes.AddGene(selectedNegativeGene, xenogene: true);
                }
            }
        }

        private void HandleRejectionOutcome(Pawn pawn)
        {
            if (HasLinkedFacility(InternalDefOf.VQEA_ArchiteRecycler))
            {
                Thing capsule = ThingMaker.MakeThing(ThingDefOf.ArchiteCapsule);
                GenPlace.TryPlaceThing(capsule, InteractionCell, Map, ThingPlaceMode.Near);
            }
            Find.LetterStack.ReceiveLetter("VQEA_LetterLabel_Rejection".Translate(),
                "VQEA_LetterDesc_Rejection".Translate(pawn.Named("PAWN")),
                LetterDefOf.NegativeEvent, pawn);
            ApplyInjectionComa(pawn);
        }

        private void HandleMutationOutcome(Pawn originalPawn, PawnKindDef mutatedPawnKind)
        {
            originalPawn.Destroy();
            Pawn mutatedPawn = PawnGenerator.GeneratePawn(mutatedPawnKind, Faction.OfPlayer);
            mutatedPawn.Name = originalPawn.Name;
            GenSpawn.Spawn(mutatedPawn, InteractionCell, Map, Rot4.North);
            mutatedPawn.mindState.mentalStateHandler.TryStartMentalState(InternalDefOf.VQEA_MutantBerserk, forceWake: true);
            Find.LetterStack.ReceiveLetter("VQEA_LetterLabel_Mutation".Translate(),
                "VQEA_LetterDesc_Mutation".Translate(originalPawn.Named("PAWN"), mutatedPawnKind.LabelCap), LetterDefOf.ThreatBig, originalPawn);
        }

        private void FindAndAddSideEffectGene(Pawn pawn)
        {
            int targetMetabolism = GetTargetMetabolismEfficiency();
            for (int metabolism = targetMetabolism; metabolism <= 10; metabolism++)
            {
                var genesWithMetabolism = GetFilteredGenes(pawn, g => g.biostatMet != 0 && g.biostatMet == metabolism);

                if (genesWithMetabolism.Any())
                {
                    GeneDef selectedGene = null;
                    if (HasLinkedFacility(InternalDefOf.VQEA_AberrationRedirector) && genesWithMetabolism.Count >= 2)
                    {
                        var availableGenes = genesWithMetabolism.ToList();
                        var gene1 = availableGenes.RandomElement();
                        availableGenes.Remove(gene1);
                        var gene2 = availableGenes.RandomElement();
                        Find.WindowStack.Add(new Dialog_MessageBox("VQEA_ChooseSideEffectGene".Translate(), gene1.LabelCap, () => { pawn.genes.AddGene(gene1, xenogene: true); generatedSideEffectGene = gene1; ApplyInjectionComa(pawn); }, gene2.LabelCap, () => { pawn.genes.AddGene(gene2, xenogene: true); generatedSideEffectGene = gene2; ApplyInjectionComa(pawn); }));
                        return;
                    }
                    else
                    {
                        selectedGene = genesWithMetabolism.RandomElement();
                    }

                    pawn.genes.AddGene(selectedGene, xenogene: true);
                    generatedSideEffectGene = selectedGene;
                    return;
                }
            }
            for (int metabolism = targetMetabolism - 1; metabolism >= -5; metabolism--)
            {
                var genesWithMetabolism = GetFilteredGenes(pawn, g => g.biostatMet != 0 && g.biostatMet == metabolism);

                if (genesWithMetabolism.Any())
                {
                    GeneDef selectedGene = null;
                    if (HasLinkedFacility(InternalDefOf.VQEA_AberrationRedirector) && genesWithMetabolism.Count >= 2)
                    {
                        var availableGenes = genesWithMetabolism.ToList();
                        var gene1 = availableGenes.RandomElement();
                        availableGenes.Remove(gene1);
                        var gene2 = availableGenes.RandomElement();
                        Find.WindowStack.Add(new Dialog_MessageBox("VQEA_ChooseSideEffectGene".Translate(), gene1.LabelCap, () => { pawn.genes.AddGene(gene1, xenogene: true); generatedSideEffectGene = gene1; ApplyInjectionComa(pawn); }, gene2.LabelCap, () => { pawn.genes.AddGene(gene2, xenogene: true); generatedSideEffectGene = gene2; ApplyInjectionComa(pawn); }));
                        return;
                    }
                    else
                    {
                        selectedGene = genesWithMetabolism.RandomElement();
                    }

                    pawn.genes.AddGene(selectedGene, xenogene: true);
                    generatedSideEffectGene = selectedGene;
                    return;
                }
            }
        }

        public int GetTargetMetabolismEfficiency()
        {
            int baseMetabolism = def.GetModExtension<ArchogenInjectorExtension>().baseSideEffectMetabolism;
            var compAffectedByFacilities = this.TryGetComp<CompAffectedByFacilities>();
            if (compAffectedByFacilities != null)
            {
                foreach (var facility in compAffectedByFacilities.LinkedFacilitiesListForReading)
                {
                    var facilityExtension = facility.def.GetModExtension<ArchiteLabFacilityExtension>();
                    if (facilityExtension != null)
                    {
                        baseMetabolism += facilityExtension.sideEffectMetabolismOffset;
                    }
                }
            }
            return baseMetabolism;
        }

        private void ApplyInjectionComa(Pawn pawn)
        {
            var comaHediff = pawn.health.AddHediff(InternalDefOf.VQEA_InjectionComa);
            var comp = comaHediff.TryGetComp<HediffComp_Disappears>();
            if (comp != null)
            {
                int comaDuration = GetExpectedComaDuration();
                comp.ticksToDisappear = comaDuration;
            }
        }

        public void CalculateOutcome()
        {
            var baseWeights = new Dictionary<ArchiteInjectionOutcomeDef, int>();
            foreach (var outcome in DefDatabase<ArchiteInjectionOutcomeDef>.AllDefs)
            {
                baseWeights[outcome] = outcome.baseWeight;
            }
            ApplyFacilityModifiers(baseWeights);
            int totalWeight = baseWeights.Values.Sum();
            outcome = baseWeights.Keys.RandomElementByWeight(w => baseWeights[w]);
        }

        private Dictionary<ArchiteInjectionOutcomeDef, float> CalculateOutcomeChances()
        {
            var baseWeights = new Dictionary<ArchiteInjectionOutcomeDef, int>();
            foreach (var outcome in DefDatabase<ArchiteInjectionOutcomeDef>.AllDefs)
            {
                baseWeights[outcome] = outcome.baseWeight;
            }
            ApplyFacilityModifiers(baseWeights);
            float totalWeight = baseWeights.Values.Sum();
            var chances = new Dictionary<ArchiteInjectionOutcomeDef, float>();
            foreach (var kvp in baseWeights)
            {
                chances[kvp.Key] = kvp.Value / totalWeight;
            }
            return chances;
        }

        private int GetGeneticComplexity(Pawn pawn)
        {
            if (pawn.genes != null)
            {
                return pawn.genes.GenesListForReading.Count;
            }
            return 0;
        }

        private void ApplyFacilityModifiers(Dictionary<ArchiteInjectionOutcomeDef, int> weights)
        {
            var successOutcome = InternalDefOf.VQEA_ArchiteInjection_Success;
            var hasComplexityHarmonizer = false;
            var compAffectedByFacilities = this.TryGetComp<CompAffectedByFacilities>();
            if (compAffectedByFacilities != null)
            {
                hasComplexityHarmonizer = compAffectedByFacilities.LinkedFacilitiesListForReading.Any(f => f.def == InternalDefOf.VQEA_ComplexityHarmonizer);
            }
            if (selectedPawn != null && !hasComplexityHarmonizer)
            {
                int geneticComplexity = GetGeneticComplexity(selectedPawn);
                int complexityPenalty = Mathf.Min(geneticComplexity, weights[successOutcome]);
                weights[successOutcome] -= complexityPenalty;
            }

            if (compAffectedByFacilities != null)
            {
                foreach (var facility in compAffectedByFacilities.LinkedFacilitiesListForReading)
                {
                    var facilityExtension = facility.def.GetModExtension<ArchiteLabFacilityExtension>();
                    if (facilityExtension != null)
                    {
                        if (facility.def != InternalDefOf.VQEA_ComplexityHarmonizer)
                        {
                            weights[successOutcome] += facilityExtension.successWeightOffset;
                        }

                        var rejectionOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Rejection);
                        if (rejectionOutcome != null)
                        {
                            weights[rejectionOutcome] = Mathf.Max(0, weights[rejectionOutcome] + facilityExtension.rejectionWeightOffset);
                        }

                        var splicelingOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Mutation && o.pawnKind == InternalDefOf.VQEA_Spliceling);
                        if (splicelingOutcome != null)
                        {
                            weights[splicelingOutcome] += facilityExtension.splicelingWeightOffset;
                        }

                        var splicehulkOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Mutation && o.pawnKind == InternalDefOf.VQEA_Splicehulk);
                        if (splicehulkOutcome != null)
                        {
                            weights[splicehulkOutcome] += facilityExtension.splicehulkWeightOffset;
                        }

                        var splicefiendOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Mutation && o.pawnKind == InternalDefOf.VQEA_Splicefiend);
                        if (splicefiendOutcome != null)
                        {
                            weights[splicefiendOutcome] += facilityExtension.splicefiendWeightOffset;
                        }
                    }
                }

                bool hasMutagenInhibitorCore = compAffectedByFacilities.LinkedFacilitiesListForReading.Any(f => f.def == InternalDefOf.VQEA_MutagenInhibitorCore);
                if (hasMutagenInhibitorCore)
                {
                    var mutationOutcomes = weights.Keys.Where(o => o.outcomeType == OutcomeType.Mutation).ToList();
                    foreach (var outcome in mutationOutcomes)
                    {
                        weights[outcome] = 0; // This guarantees mutation chance is zero, overriding all previous modifiers.
                    }
                    var rejectionOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Rejection);
                    if (rejectionOutcome != null)
                    {
                        weights[rejectionOutcome] = Mathf.Max(0, weights[rejectionOutcome] + 25);
                    }
                }
            }
        }

        public bool HasLinkedFacility(ThingDef facilityDef)
        {
            var compAffectedByFacilities = this.TryGetComp<CompAffectedByFacilities>();
            if (compAffectedByFacilities != null)
            {
                return compAffectedByFacilities.LinkedFacilitiesListForReading.Any(facility => facility.def == facilityDef);
            }
            return false;
        }

        public int GetExpectedInfusionDuration()
        {
            int baseTicks = def.GetModExtension<ArchogenInjectorExtension>().baseInjectionTicks;
            var compAffectedByFacilities = this.TryGetComp<CompAffectedByFacilities>();
            if (compAffectedByFacilities != null)
            {
                foreach (var facility in compAffectedByFacilities.LinkedFacilitiesListForReading)
                {
                    var facilityExtension = facility.def.GetModExtension<ArchiteLabFacilityExtension>();
                    if (facilityExtension != null)
                    {
                        baseTicks += facilityExtension.injectionTicksOffset;
                        if (facility.def == InternalDefOf.VQEA_ComplexityHarmonizer && selectedPawn != null)
                        {
                            int geneticComplexity = GetGeneticComplexity(selectedPawn);
                            baseTicks += geneticComplexity * 2500;
                        }
                    }
                }
            }
            return Mathf.Max(60000, baseTicks);
        }

        public int GetExpectedComaDuration()
        {
            int baseTicks = def.GetModExtension<ArchogenInjectorExtension>().baseComaTicks;
            var compAffectedByFacilities = this.TryGetComp<CompAffectedByFacilities>();
            if (compAffectedByFacilities != null)
            {
                foreach (var facility in compAffectedByFacilities.LinkedFacilitiesListForReading)
                {
                    var facilityExtension = facility.def.GetModExtension<ArchiteLabFacilityExtension>();
                    if (facilityExtension != null)
                    {
                        baseTicks += facilityExtension.comaTicksOffset;
                    }
                }
            }
            return Mathf.Max(6000, baseTicks);
        }

        public int GetGeneticComplexityForUI()
        {
            if (selectedPawn != null && selectedPawn.genes != null)
            {
                return selectedPawn.genes.GenesListForReading.Count;
            }
            return 0;
        }

        public float GetOutcomeChance(ArchiteInjectionOutcomeDef outcomeDef)
        {
            var chances = CalculateOutcomeChances();
            return chances[outcomeDef];
        }

        public List<ThingDef> GetLinkedLabEquipment()
        {
            var compAffectedByFacilities = this.TryGetComp<CompAffectedByFacilities>();
            if (compAffectedByFacilities != null)
            {
                return compAffectedByFacilities.LinkedFacilitiesListForReading.Select(f => f.def).ToList();
            }
            return new List<ThingDef>();
        }
    }
}
