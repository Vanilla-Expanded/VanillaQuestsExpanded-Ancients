using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace VanillaQuestsExpandedAncients
{

    public class DamageWorker_Scalpel : DamageWorker_Cut
    {


        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);

            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker != null)
            {
                float extraDamage = attacker.skills.GetSkill(SkillDefOf.Medicine).Level;
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, extraDamage, 0f, -1f, attacker, null, dinfo.Weapon, DamageInfo.SourceCategory.ThingOrUnknown));

            }



        }
    }
}