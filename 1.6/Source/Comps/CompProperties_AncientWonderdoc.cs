using System.Collections.Generic;
using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class CompProperties_AncientWonderdoc : CompProperties
    {
        public List<WonderdocCycle> cycles = new List<WonderdocCycle>();

        public CompProperties_AncientWonderdoc()
        {
            compClass = typeof(CompAncientWonderdoc);
        }
    }
}