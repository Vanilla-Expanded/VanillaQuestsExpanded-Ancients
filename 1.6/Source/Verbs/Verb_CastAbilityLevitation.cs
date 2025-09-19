
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;


namespace VanillaQuestsExpandedAncients
{
    public class Verb_CastAbilityLevitation : Verb_CastAbility
    {



        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                return DoJump(CasterPawn, currentTarget, ReloadableCompSource, verbProps);
            }
            return false;
        }

        public static bool DoJump(Pawn pawn, LocalTargetInfo currentTarget, CompApparelReloadable comp, VerbProperties verbProps, Ability triggeringAbility = null, LocalTargetInfo target = default(LocalTargetInfo), ThingDef pawnFlyerOverride = null)
        {

            IntVec3 position = pawn.Position;
            IntVec3 cell = currentTarget.Cell;
            Map map = pawn.Map;
            bool flag = Find.Selector.IsSelected(pawn);
            PawnFlyer pawnFlyer = (PawnFlyer)PawnFlyer.MakeFlyer(InternalDefOf.VQEA_PawnLevitator, pawn, cell, verbProps.flightEffecterDef, verbProps.soundLanding, verbProps.flyWithCarriedThing, null, triggeringAbility, target);

            if (pawnFlyer != null)
            {
                FleckMaker.ThrowDustPuff(position.ToVector3Shifted() + Gen.RandomHorizontalVector(0.5f), map, 2f);
                GenSpawn.Spawn(pawnFlyer, cell, map);
                if (flag)
                {
                    Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
                }
                return true;
            }
            return false;
        }

     






    }
}
