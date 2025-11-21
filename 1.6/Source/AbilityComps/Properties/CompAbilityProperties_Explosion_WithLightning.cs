
using RimWorld;
using Verse;
namespace VanillaQuestsExpandedAncients
{
    public class CompAbilityProperties_Explosion_WithLightning:CompProperties_AbilityExplosion
    {
       

        public int lightningAmount = 1;
        public int ticksBetween = 8;


        public CompAbilityProperties_Explosion_WithLightning()
        {
            compClass = typeof(CompAbilityEffect_Explosion_WithLightning);
        }
    }
}