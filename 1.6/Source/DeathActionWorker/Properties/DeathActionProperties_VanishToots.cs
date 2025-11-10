
using Verse;
namespace VanillaQuestsExpandedAncients
{

    public class DeathActionProperties_VanishToots : DeathActionProperties
    {
        public FleckDef fleck;

        public ThingDef filth;

        public IntRange filthCountRange = IntRange.One;

        public float gasAmount;

        public DeathActionProperties_VanishToots()
        {
            workerClass = typeof(DeathActionWorker_VanishToots);
        }
    }
}