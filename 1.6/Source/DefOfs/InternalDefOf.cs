using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
namespace VanillaQuestsExpandedAncients
{
	[DefOf]
	public static class InternalDefOf
	{
		static InternalDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(InternalDefOf));
		}

		public static ThingDef VQEA_PawnLevitator;
		public static ThingDef VQEA_AncientPipelineJunction_Off, VQEA_AncientPipelineJunction_On;
		public static ThingDef VQEA_ArchogenInjector;
		public static ThingDef VQEA_NeurostabilizerArray;
		public static ThingDef VQEA_CognitiveRecoveryArray;
		public static ThingDef VQEA_RapidInfusionPump;
		public static ThingDef VQEA_ArchiteRecycler;
		public static ThingDef VQEA_GenomicAttenuator;
		public static ThingDef VQEA_RejectionBufferCoil;
		public static ThingDef VQEA_SpliceframeUplink;
		public static ThingDef VQEA_MutagenInhibitorCore;
		public static ThingDef VQEA_TraitSelectionPrism;
		public static ThingDef VQEA_AberrationRedirector;
		public static ThingDef VQEA_ComplexityHarmonizer;
		public static ThingDef VQEA_ArchitePathingArray;
		public static ThingDef VQEA_Apparel_PatientGown;
		public static ThingCategoryDef VQEA_BuildingsLab;
		public static ArchiteInjectionOutcomeDef VQEA_ArchiteInjection_Success;
		public static ArchiteInjectionOutcomeDef VQEA_ArchiteInjection_Rejection;
		public static ArchiteInjectionOutcomeDef VQEA_ArchiteInjection_Spliceling;
		public static ArchiteInjectionOutcomeDef VQEA_ArchiteInjection_Splicehulk;
		public static ArchiteInjectionOutcomeDef VQEA_ArchiteInjection_Splicefiend;

		public static GeneDef VQEA_Serene;
		public static GeneDef VQEA_SubstanceImpervious;
		public static GeneDef VQEA_MasterfulAnimals;
		public static GeneDef VQEA_MasterfulArtistic;
		public static GeneDef VQEA_MasterfulConstruction;
		public static GeneDef VQEA_MasterfulMedical;
		public static GeneDef VQEA_MasterfulMelee;
		public static GeneDef VQEA_MasterfulMining;
		public static GeneDef VQEA_MasterfulPlants;
		public static GeneDef VQEA_MasterfulShooting;
		public static GeneDef VQEA_MasterfulSocial;
        public static GeneDef VQEA_MasterfulCrafting;
        public static GeneDef VQEA_MasterfulCooking;
        public static GeneDef VQEA_Electromagnetized;

        public static StatDef CookSpeed;
        public static StatDef ButcheryFleshEfficiency;

        public static JoyKindDef VQEA_AnimalRelaxation;

		public static HediffDef VQEA_InjectionComa;
		public static HediffDef VQEA_HemocollapseSyndrome;
        public static HediffDef VQEA_Regenerating;

        public static BodyPartDef Brain;
        public static BodyPartDef Reactor;

        public static PawnKindDef VQEA_Spliceling;
		public static PawnKindDef VQEA_Splicehulk;
		public static PawnKindDef VQEA_Splicefiend;

		public static MentalStateDef VQEA_MutantBerserk;

		public static SoundDef VQEA_Foosball_Ambience;

		public static EffecterDef CocoonWakingUp;
        public static EffecterDef VQEA_Bubbles;

        public static JobDef VQEA_CarryToBioBattery;
        public static HediffDef BadBack;
        public static HediffDef Frail;
        public static HediffDef Cataract;
        public static HediffDef Alzheimers;
        public static HediffDef Malaria;
        public static HediffDef SleepingSickness;
        public static HediffDef Flu;
        public static HediffDef Plague;
        public static HediffDef GutWorms;
        public static HediffDef MuscleParasites;
        public static SoundDef SubcoreSoftscanner_Start;
        public static SoundDef SubcoreSoftscanner_Working;
        public static SoundDef VQEA_PneumaticLaunch;
		public static SoundDef VQEA_PneumaticArrival;
		public static TraitDef VQE_IdealPatient;
		public static RulePackDef VQE_ExperimentMaleNames;
		public static RulePackDef VQE_ExperimentFemaleNames;
		public static RulePackDef VQE_ExperimentLastNames;

		public static SitePartDef VQEA_AncientLabComplexSite;
		public static SitePartDef VQEA_ArchiteControlVaultSite;
		public static SitePartDef VQEA_SpliceframeBlacksiteSite;
		public static SitePartDef VQEA_InhibitorResearchLabSite;
		public static SitePartDef VQEA_ArchiteArraySite;
		public static ThingDef VQEA_LockedVaultDoor;
		public static ThingDef VQEA_AncientBroadcastingStation;
		public static ThingDef VQEA_AncientWonderdoc;
		public static ThingDef VQEA_BustedAncientWonderdoc;
		public static GameConditionDef VQEA_AncientComplex;
		public static ThingDef VQEA_AncientLaboratoryCasket;
		public static ThingDef VQEA_CandidateCryptosleepCasket;
		public static StructureSetDef VQEA_SealedVaultStartStructure;
		public static PawnKindDef VQE_Experiment;
		public static MapGeneratorDef VQEA_SealedVault;
		public static PawnKindDef VQE_Patient;
    }
}
