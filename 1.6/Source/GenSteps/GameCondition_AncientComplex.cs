using RimWorld;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class GameCondition_AncientComplex : GameCondition_ForceWeather
    {
        public override WeatherDef ForcedWeather()
        {
            return InternalDefOf.VQEA_AncientComplex.weatherDef;
        }
    }
}
