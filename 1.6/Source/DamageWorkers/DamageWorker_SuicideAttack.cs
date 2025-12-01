using RimWorld;
using Verse;
namespace VanillaQuestsExpandedAncients
{

    public class DamageWorker_SuicideAttack : DamageWorker_Bite
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {

        
            Pawn pawn = dinfo.Instigator as Pawn;
            if (pawn != null) {
                pawn.health?.AddHediff(InternalDefOf.VQEA_SuicideHediff);
            }
            return base.Apply(dinfo,victim);
        }
    }
}
