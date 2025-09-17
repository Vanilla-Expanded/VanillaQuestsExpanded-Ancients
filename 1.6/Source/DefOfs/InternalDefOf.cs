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
        public static JoyKindDef VQEA_AnimalRelaxation;

		public static BodyPartDef Brain;

    }
}
