using RimWorld;
using Verse;
namespace VanillaQuestsExpandedAncients
{

    public class DamageWorker_SuicideAttack : DamageWorker_Bite
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {

           dinfo.Instigator?.Kill();

            return base.Apply(dinfo,victim);
        }
    }
}
