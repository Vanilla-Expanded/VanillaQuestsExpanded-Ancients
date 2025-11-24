
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
namespace VanillaQuestsExpandedAncients
{
    public class CompAbilityEffect_Explosion_WithLightning:CompAbilityEffect_Explosion
    {
        private new CompAbilityProperties_Explosion_WithLightning Props => (CompAbilityProperties_Explosion_WithLightning)props;

        private Pawn Pawn => parent.pawn;

        public bool startLightning = false;
        public int lightningCounter = 0;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (Outdoors(Pawn))
            {
                startLightning = true;
                lightningCounter = Props.lightningAmount;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (startLightning)
            {
                if (Pawn.IsHashIntervalTick(Props.ticksBetween))
                {


                    RCellFinder.TryFindRandomCellNearWith(Pawn.Position, (IntVec3 pos) => pos.InBounds(Pawn.Map), Pawn.Map, out IntVec3 result,7, 10);
                    Pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Pawn.Map, result));
                    




                    lightningCounter--;
                    if (lightningCounter < 0) {
                        startLightning = false;
                    }
                }

            }
        }


        private static bool Outdoors(Thing thing)
        {
            RoofDef roof = thing.Position.GetRoof(thing.Map);
            if (roof != null && (roof.isNatural || roof.isThickRoof))
            {
                return false;
            }
            return thing.Position.GetRoom(thing.Map)?.PsychologicallyOutdoors ?? false;
        }

    }
}