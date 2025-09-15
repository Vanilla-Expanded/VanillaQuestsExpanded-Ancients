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

    }
}
