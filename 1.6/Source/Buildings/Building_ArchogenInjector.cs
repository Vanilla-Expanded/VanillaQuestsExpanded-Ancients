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
        WaitingForPawn,
        PawnInside,
        WaitingForCapsule,
        Injecting,
        Complete
    }

    [StaticConstructorOnStartup]
    public class Building_ArchogenInjector : Building_Enterable, IThingHolder
    {
        private bool processConfirmed = false;
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
        private static readonly SoundDef EjectSoundSuccess = SoundDefOf.CryptosleepCasket_Eject;
        private static readonly SoundDef EjectSoundFail = SoundDefOf.CryptosleepCasket_Eject;
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
                if (!PowerOn)
                {
                    return ArchiteInjectorState.Inactive;
                }
                if (Occupant == null)
                {
                    return ArchiteInjectorState.WaitingForPawn;
                }
                if (!processConfirmed)
                {
                    return ArchiteInjectorState.PawnInside;
                }
                if (!AllRequiredIngredientsLoaded)
                {
                    return ArchiteInjectorState.WaitingForCapsule;
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
                if (pawn.health.hediffSet.HasHediff(InternalDefOf.VQEA_InjectionComa))
                {
                    return "VQEA_HasInjectionComa".Translate();
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
                Find.WindowStack.Add(new Window_ArchiteInjection(this));
            }
        }

        public bool CanAcceptIngredient(Thing thing)
        {
            return GetRequiredCountOf(thing.def) > 0;
        }

        public void CancelProcess()
        {
            if (this.State == ArchiteInjectorState.Injecting && !HasLinkedFacility(InternalDefOf.VQEA_ArchiteRecycler))
            {
                Thing capsule = innerContainer.FirstOrDefault(t => t.def == ThingDefOf.ArchiteCapsule);
                if (capsule != null)
                {
                    capsule.Destroy();
                }
            }
            innerContainer.TryDropAll(this.InteractionCell, this.Map, ThingPlaceMode.Near);
            Reset();
        }

        private void Reset()
        {
            selectedPawn = null;
            processConfirmed = false;
            ticksRemaining = 0;
            totalInjectionTime = 0;
            outcome = null;
            innerContainer.ClearAndDestroyContents();
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
            if (processConfirmed && totalInjectionTime == 0)
            {
                if (AllRequiredIngredientsLoaded)
                {
                    StartInjection();
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
                    FinishInjection();
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
                CancelProcess();
            }
            if (selectedPawn != null && (selectedPawn.Dead || selectedPawn.MapHeld != Map))
            {
                selectedPawn = null;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (selectedPawn == null)
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
                else if (State == ArchiteInjectorState.WaitingForCapsule || State == ArchiteInjectorState.PawnInside)
                {
                    command_Action2.Disable("VQEA_ArchogenInjectorWaitingForCapsule".Translate());
                }
                yield return command_Action2;
            }
            if (State == ArchiteInjectorState.PawnInside || State == ArchiteInjectorState.Injecting)
            {
                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = "CommandCancelLoad".Translate();
                command_Action3.defaultDesc = "CommandCancelLoadDesc".Translate();
                command_Action3.icon = CancelIcon;
                command_Action3.action = delegate
                {
                    CancelProcess();
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
                case ArchiteInjectorState.PawnInside:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("VQEA_ArchogenInjectorPawnInside".Translate());
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
            Scribe_Values.Look(ref processConfirmed, "processConfirmed", defaultValue: false);
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

        public void ConfirmInjection()
        {
            processConfirmed = true;
        }

        private void FinishInjection()
        {
            Pawn occupant = Occupant;
            if (occupant == null) return;

            if (outcome == InternalDefOf.VQEA_ArchiteInjection_Success)
            {
                EjectSoundSuccess.PlayOneShot(new TargetInfo(Position, Map));
                HandleSuccessOutcome(occupant);
            }
            else if (outcome == InternalDefOf.VQEA_ArchiteInjection_Rejection)
            {
                EjectSoundFail.PlayOneShot(new TargetInfo(Position, Map));
                HandleRejectionOutcome(occupant);
            }
            else
            {
                EjectSoundFail.PlayOneShot(new TargetInfo(Position, Map));
                HandleMutationOutcome(occupant, outcome.pawnKind);
            }
        }

        private List<GeneDef> GetFilteredGenes(Pawn pawn, Func<GeneDef, bool> additionalFilter)
        {
            return DefDatabase<GeneDef>.AllDefs.Where(g =>
                additionalFilter(g) &&
                (g.prerequisite == null || pawn.genes.HasActiveGene(g.prerequisite)) &&
                !pawn.genes.HasActiveGene(g) &&
                !pawn.genes.GenesListForReading.Any(existing => existing.def.ConflictsWith(g)) &&
                g.displayCategory?.defName != "VRE_Morphs"
            ).ToList();
        }
        private void HandleSuccessOutcome(Pawn pawn)
        {
            List<GeneDef> architeGeneChoices = new List<GeneDef>();
            var allArchiteGenes = GetFilteredGenes(pawn, g => g.biostatArc > 0);
            if (allArchiteGenes.Any() && HasLinkedFacility(InternalDefOf.VQEA_TraitSelectionPrism))
            {
                var gene1 = allArchiteGenes.RandomElement();
                architeGeneChoices.Add(gene1);
                var availableForSecond = allArchiteGenes.Where(g => g != gene1).ToList();
                if (availableForSecond.Any())
                {
                    architeGeneChoices.Add(availableForSecond.RandomElement());
                }
            }
            List<GeneDef> sideEffectGeneChoices = new List<GeneDef>();
            int targetMetabolism = GetTargetMetabolismEfficiency();
            for (int metabolism = targetMetabolism; metabolism <= 10; metabolism++)
            {
                var genesWithMetabolism = GetFilteredGenes(pawn, g => g.biostatMet != 0 && g.biostatMet == metabolism && !architeGeneChoices.Any(ag => ag.ConflictsWith(g)));
                if (genesWithMetabolism.Any() && HasLinkedFacility(InternalDefOf.VQEA_AberrationRedirector) && genesWithMetabolism.Count >= 2)
                {
                    var gene1 = genesWithMetabolism.RandomElement();
                    sideEffectGeneChoices.Add(gene1);
                    var availableForSecond = genesWithMetabolism.Where(g => g != gene1).ToList();
                    if (availableForSecond.Any())
                    {
                        sideEffectGeneChoices.Add(availableForSecond.RandomElement());
                    }
                    break;
                }
            }
            if (architeGeneChoices.Any() || sideEffectGeneChoices.Any())
            {
                Find.WindowStack.Add(new Window_GeneChoice(architeGeneChoices, sideEffectGeneChoices, delegate (GeneDef selectedArchite, GeneDef selectedSideEffect)
                {
                    if (selectedArchite != null)
                    {
                        pawn.genes.AddGene(selectedArchite, xenogene: true);
                        generatedArchiteGene = selectedArchite;
                    }
                    else if (allArchiteGenes.Any())
                    {
                        var randomArchite = allArchiteGenes.RandomElement();
                        pawn.genes.AddGene(randomArchite, xenogene: true);
                        generatedArchiteGene = randomArchite;
                    }
                    if (selectedSideEffect != null)
                    {
                        pawn.genes.AddGene(selectedSideEffect, xenogene: true);
                        generatedSideEffectGene = selectedSideEffect;
                    }
                    else
                    {
                        AddRandomSideEffectGene(pawn);
                    }
                    PostGeneSelectionSuccess(pawn);
                }));
            }
            else
            {
                if (allArchiteGenes.Any())
                {
                    var randomArchite = allArchiteGenes.RandomElement();
                    pawn.genes.AddGene(randomArchite, xenogene: true);
                    generatedArchiteGene = randomArchite;
                }
                AddRandomSideEffectGene(pawn);
                PostGeneSelectionSuccess(pawn);
            }
        }

        private void AddRandomSideEffectGene(Pawn pawn)
        {
            int targetMetabolism = GetTargetMetabolismEfficiency();
            for (int metabolism = targetMetabolism; metabolism <= 10; metabolism++)
            {
                var genesWithMetabolism = GetFilteredGenes(pawn, g => g.biostatMet != 0 && g.biostatMet == metabolism);
                if (genesWithMetabolism.Any())
                {
                    var selectedGene = genesWithMetabolism.RandomElement();
                    pawn.genes.AddGene(selectedGene, xenogene: true);
                    generatedSideEffectGene = selectedGene;
                    return;
                }
            }
        }

        private void PostGeneSelectionSuccess(Pawn pawn)
        {
            if (HasLinkedFacility(InternalDefOf.VQEA_ArchitePathingArray) && Rand.Chance(0.25f))
            {
                var remainingArchiteGenes = GetFilteredGenes(pawn, g => g.biostatArc > 0);
                if (remainingArchiteGenes.Any())
                {
                    var secondArchiteGene = remainingArchiteGenes.RandomElement();
                    pawn.genes.AddGene(secondArchiteGene, xenogene: true);
                }
            }
            if (HasLinkedFacility(InternalDefOf.VQEA_ArchitePathingArray) && Rand.Chance(0.5f))
            {
                var negativeGenes = GetFilteredGenes(pawn, g => g.biostatArc <= 0 && g.biostatMet > 0);
                if (negativeGenes.Any())
                {
                    var selectedNegativeGene = negativeGenes.RandomElement();
                    pawn.genes.AddGene(selectedNegativeGene, xenogene: true);
                }
            }
            FinalizeSuccess(pawn);
        }
        private void FinalizeSuccess(Pawn pawn)
        {
            if (innerContainer.Contains(pawn))
            {
                innerContainer.TryDrop(pawn, this.InteractionCell, this.Map, ThingPlaceMode.Near, 1, out Thing _);
            }

            if (generatedSideEffectGene != null)
            {
                Find.LetterStack.ReceiveLetter("VQEA_LetterLabel_Success".Translate(), "VQEA_LetterDesc_Success".Translate(pawn.Named("PAWN"), generatedArchiteGene.Named("ARCHITEGENE"), generatedSideEffectGene.Named("SIDEEFFECTGENE")), LetterDefOf.PositiveEvent, pawn);
            }
            else
            {
                Find.LetterStack.ReceiveLetter("VQEA_LetterLabel_Success".Translate(), "VQEA_LetterDesc_SuccessNoSideEffect".Translate(pawn.Named("PAWN"), generatedArchiteGene.Named("ARCHITEGENE")), LetterDefOf.PositiveEvent, pawn);
            }
            ApplyInjectionComa(pawn);
            Reset();
        }

        private void HandleRejectionOutcome(Pawn pawn)
        {
            if (HasLinkedFacility(InternalDefOf.VQEA_ArchiteRecycler))
            {
                Thing capsule = ThingMaker.MakeThing(ThingDefOf.ArchiteCapsule);
                GenPlace.TryPlaceThing(capsule, InteractionCell, Map, ThingPlaceMode.Near);
            }

            innerContainer.TryDrop(pawn, this.InteractionCell, this.Map, ThingPlaceMode.Near, 1, out Thing _);

            Find.LetterStack.ReceiveLetter("VQEA_LetterLabel_Rejection".Translate(),
                "VQEA_LetterDesc_Rejection".Translate(pawn.Named("PAWN")),
                LetterDefOf.NegativeEvent, pawn);
            ApplyInjectionComa(pawn);

            Reset();
        }

        private void HandleMutationOutcome(Pawn originalPawn, PawnKindDef mutatedPawnKind)
        {
            Name name = originalPawn.Name;
            originalPawn.Destroy();

            Pawn mutatedPawn = PawnGenerator.GeneratePawn(mutatedPawnKind, Faction.OfPlayer);
            mutatedPawn.Name = name;
            GenSpawn.Spawn(mutatedPawn, InteractionCell, Map, Rot4.North);
            mutatedPawn.mindState.mentalStateHandler.TryStartMentalState(InternalDefOf.VQEA_MutantBerserk, forceWake: true);
            Find.LetterStack.ReceiveLetter("VQEA_LetterLabel_Mutation".Translate(),
                "VQEA_LetterDesc_Mutation".Translate(originalPawn.Named("PAWN"), mutatedPawnKind.LabelCap), LetterDefOf.ThreatBig, mutatedPawn);

            Reset();
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
            if (totalWeight == 0) return baseWeights.ToDictionary(kvp => kvp.Key, kvp => 0f);
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

            if (!hasComplexityHarmonizer)
            {
                int geneticComplexity = GetGeneticComplexity(Occupant);
                weights[successOutcome] = Mathf.Max(0, weights[successOutcome] - geneticComplexity);
            }

            if (compAffectedByFacilities != null)
            {
                foreach (var facility in compAffectedByFacilities.LinkedFacilitiesListForReading)
                {
                    var facilityExtension = facility.def.GetModExtension<ArchiteLabFacilityExtension>();
                    if (facilityExtension != null)
                    {
                        weights[successOutcome] = Mathf.Max(0, weights[successOutcome] + facilityExtension.successWeightOffset);

                        var rejectionOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Rejection);
                        if (rejectionOutcome != null)
                        {
                            weights[rejectionOutcome] = Mathf.Max(0, weights[rejectionOutcome] + facilityExtension.rejectionWeightOffset);
                        }

                        var splicelingOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Mutation && o.pawnKind == InternalDefOf.VQEA_Spliceling);
                        if (splicelingOutcome != null)
                        {
                            weights[splicelingOutcome] = Mathf.Max(0, weights[splicelingOutcome] + facilityExtension.splicelingWeightOffset);
                        }

                        var splicehulkOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Mutation && o.pawnKind == InternalDefOf.VQEA_Splicehulk);
                        if (splicehulkOutcome != null)
                        {
                            weights[splicehulkOutcome] = Mathf.Max(0, weights[splicehulkOutcome] + facilityExtension.splicehulkWeightOffset);
                        }

                        var splicefiendOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Mutation && o.pawnKind == InternalDefOf.VQEA_Splicefiend);
                        if (splicefiendOutcome != null)
                        {
                            weights[splicefiendOutcome] = Mathf.Max(0, weights[splicefiendOutcome] + facilityExtension.splicefiendWeightOffset);
                        }
                    }
                }

                bool hasMutagenInhibitorCore = compAffectedByFacilities.LinkedFacilitiesListForReading.Any(f => f.def == InternalDefOf.VQEA_MutagenInhibitorCore);
                if (hasMutagenInhibitorCore)
                {
                    var mutationOutcomes = weights.Keys.Where(o => o.outcomeType == OutcomeType.Mutation).ToList();
                    foreach (var mOutcome in mutationOutcomes)
                    {
                        weights[mOutcome] = 0;
                    }
                }
            }

            if (Occupant.story.traits.HasTrait(InternalDefOf.VQE_IdealPatient))
            {
                var rejectionOutcome = weights.Keys.FirstOrDefault(o => o.outcomeType == OutcomeType.Rejection);
                if (rejectionOutcome != null)
                {
                    weights[rejectionOutcome] = 0;
                }

                var mutationOutcomes = weights.Keys.Where(o => o.outcomeType == OutcomeType.Mutation).ToList();
                foreach (var mOutcome in mutationOutcomes)
                {
                    weights[mOutcome] = 0;
                }
            }

            foreach (var apparel in Occupant.apparel.WornApparel)
            {
                if (apparel.def == InternalDefOf.VQEA_Apparel_PatientGown)
                {
                    weights[successOutcome] += 2;
                    break;
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
                    }

                    if (facility.def == InternalDefOf.VQEA_ComplexityHarmonizer)
                    {
                        int geneticComplexity = GetGeneticComplexity(Occupant);
                        baseTicks += geneticComplexity * 2500;
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
            if (Occupant != null && Occupant.genes != null)
            {
                return Occupant.genes.GenesListForReading.Count;
            }
            return 0;
        }

        public float GetOutcomeChance(ArchiteInjectionOutcomeDef outcomeDef)
        {
            var chances = CalculateOutcomeChances();
            return chances.TryGetValue(outcomeDef, out float chance) ? chance : 0f;
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
