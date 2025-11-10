using Verse;
using System.Collections.Generic;

namespace VanillaQuestsExpandedAncients
{
    public class StructurePatternOffset
    {
        public string pattern;
        public IntVec3 offset;
        public List<PawnSpawnOption> spawnPawns;
        public List<ThingSpawnOption> spawnThings;
        public bool forceSpawnEnemiesIndoor;
    }
}